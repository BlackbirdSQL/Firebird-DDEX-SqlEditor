SET TERM ^ ;

-- Begin section 1 - Main Select

EXECUTE BLOCK
	RETURNS (
		TABLE_CATALOG varchar(50), TABLE_SCHEMA varchar(50), TABLE_NAME varchar(100), TRIGGER_NAME varchar(100),
		IS_SYSTEM_OBJECT boolean, TRIGGER_TYPE bigint, IS_INACTIVE smallint, PRIORITY smallint,
		EXPRESSION blob sub_type 1, DESCRIPTION blob sub_type 1,
		DEPENDENCY_FIELDS blob sub_type 1, TRIGGER_DEPENDENCYCOUNT int, IS_IDENTITY boolean, SEQUENCE_NAME varchar(50))
AS
DECLARE DEPENDENCY_FIELD varchar(50);
DECLARE CONSTRAINT_TYPE varchar(20);
DECLARE SEGMENT_FIELD varchar(50);
-- :IDENTITY_FLAG is Firefox's interpretation of an identity field
-- We care about it if and only if :SEQUENCE_NAME is not null
DECLARE IDENTITY_FLAG int;
BEGIN
	FOR
		SELECT
			null, null, trg.rdb$relation_name, trg.rdb$trigger_name,
			(CASE WHEN trg.rdb$system_flag <> 1 THEN false ELSE true END),
			trg.rdb$trigger_type, trg.rdb$trigger_inactive, trg.rdb$trigger_sequence,
			(CASE WHEN trg.rdb$trigger_source IS NULL AND trg.rdb$trigger_blr IS NOT NULL THEN
				cast(trg.rdb$trigger_blr as blob sub_type 1)
			ELSE
				trg.rdb$trigger_source
			END),
			trg.rdb$description,
			-- Initial value of :IS_IDENTITY
			(CASE WHEN trg.rdb$flags = 1 AND trg.rdb$trigger_sequence = 1 AND trg.rdb$trigger_type = 1 THEN true ELSE false END),
			-- Initial value of :IDENTITY_FLAG and :SEQUENCE_NAME is null
			'', 0, null, null
		FROM rdb$triggers trg
		
-- End of section 1

-- Begin section 2 - Dependency select

		ORDER BY trg.rdb$trigger_name
		INTO :TABLE_CATALOG, :TABLE_SCHEMA, :TABLE_NAME, :TRIGGER_NAME,
            :IS_SYSTEM_OBJECT,
            :TRIGGER_TYPE, :IS_INACTIVE, :PRIORITY,
            :EXPRESSION,
            :DESCRIPTION,
            :IS_IDENTITY,
            :DEPENDENCY_FIELDS, :TRIGGER_DEPENDENCYCOUNT, :SEQUENCE_NAME, :IDENTITY_FLAG
	DO BEGIN
		FOR
			SELECT
                -- Swap fd_rfr.rdb$identity_type and fd_rfr.rdb$generator_name with {0} and {1} in sql and c# code respectively
				fd.rdb$field_name, fd_con.rdb$constraint_type, fd_seg.rdb$field_name, fd_rfr.rdb$identity_type, fd_rfr.rdb$generator_name
			FROM rdb$dependencies fd
			LEFT OUTER JOIN rdb$relation_constraints fd_con
				ON fd_con.rdb$relation_name = fd.rdb$depended_on_name AND fd_con.rdb$constraint_type = 'PRIMARY KEY'
			LEFT OUTER JOIN rdb$index_segments fd_seg
				ON fd_con.rdb$index_name IS NOT NULL AND fd_seg.rdb$index_name = fd_con.rdb$index_name AND fd_seg.rdb$field_name = fd.rdb$field_name
			LEFT OUTER JOIN rdb$relation_fields fd_rfr
				ON fd_seg.rdb$index_name IS NOT NULL AND fd_rfr.rdb$relation_name = fd_con.rdb$relation_name AND fd_rfr.rdb$field_name = fd_seg.rdb$field_name
			WHERE fd.rdb$field_name IS NOT NULL AND fd.rdb$dependent_name = :TRIGGER_NAME AND fd.rdb$depended_on_name = :TABLE_NAME
			INTO :DEPENDENCY_FIELD, :CONSTRAINT_TYPE, :SEGMENT_FIELD, :IDENTITY_FLAG, :SEQUENCE_NAME
		DO BEGIN
			IF (:SEGMENT_FIELD IS NULL) THEN
			BEGIN
				:IS_IDENTITY = false;
			END
			:TRIGGER_DEPENDENCYCOUNT = :TRIGGER_DEPENDENCYCOUNT + 1;
			IF (DEPENDENCY_FIELDS <> '') THEN
			BEGIN
				:DEPENDENCY_FIELDS = :DEPENDENCY_FIELDS || ', ';
			END
			:DEPENDENCY_FIELDS = :DEPENDENCY_FIELDS || TRIM(DEPENDENCY_FIELD);
		END
        -- If the trigger dependency count is zero it can't be an auto increment.
		IF (:TRIGGER_DEPENDENCYCOUNT <> 1) THEN
		BEGIN
			:IS_IDENTITY = false;
		END ELSE BEGIN
            IF (:SEQUENCE_NAME IS NOT NULL) THEN
            BEGIN
                -- There is a generator so :IDENTITY_FLAG determines if auto-increment still holds true
                IF (:IDENTITY_FLAG IS NULL OR :IDENTITY_FLAG = 0) THEN
                BEGIN
                    :IS_IDENTITY = false;
                END
            END
        END
        if (:IS_IDENTITY AND :SEQUENCE_NAME IS NULL) THEN
        BEGIN
            -- Swap Unidentifiable and {2} in sql and c# code respectively
            :SEQUENCE_NAME = 'Unidentifiable';
        END

-- End section 2 

-- Begin section 3 - Trigger row complete

        SUSPEND;            

-- End section 3

-- Finalize

	END
END
                
^
SET TERM ; ^