SET TERM ^ ;
EXECUTE BLOCK
	RETURNS (
        TABLE_CATALOG varchar(10), TABLE_SCHEMA varchar(10), TABLE_NAME varchar(50), COLUMN_NAME varchar(50),
		COLUMN_SUB_TYPE smallint,
        COLUMN_SIZE integer, NUMERIC_PRECISION integer, NUMERIC_SCALE integer, CHARACTER_MAX_LENGTH integer, CHARACTER_OCTET_LENGTH integer,
        ORDINAL_POSITION smallint, DOMAIN_CATALOG varchar(10), DOMAIN_SCHEMA varchar(1), DOMAIN_NAME varchar(50), COLUMN_DEFAULT blob sub_type 1,
        EXPRESSION blob sub_type 1, IS_COMPUTED boolean, IS_ARRAY boolean, IS_NULLABLE boolean, READONLY_FLAG smallint, FIELD_TYPE smallint,
        CHARACTER_SET_CATALOG varchar(10), CHARACTER_SET_SCHEMA varchar(10), CHARACTER_SET_NAME varchar(50), 
        COLLATION_CATALOG varchar(10), COLLATION_SCHEMA varchar(10), COLLATION_NAME varchar(50),
        DESCRIPTION varchar(50), IN_PRIMARYKEY boolean, IS_UNIQUE boolean, IS_IDENTITY boolean, SEQUENCE_GENERATOR varchar(50),
        -- [returnClause]~0~ directly after TRIGGER_NAME varchar(50)
		IDENTITY_SEED bigint, IDENTITY_INCREMENT int, IDENTITY_CURRENT bigint, PARENT_TYPE varchar(15), TRIGGER_NAME varchar(50))
AS
DECLARE PRIMARY_DEPENDENCYCOUNT int;
DECLARE TRIGGER_DEPENDENCYCOUNT int;
DECLARE IDENTITY_TYPE smallint;
DECLARE SEGMENT_FIELD varchar(50);
BEGIN
    :TRIGGER_DEPENDENCYCOUNT = 0;

	SELECT COUNT(*)
	-- [rdb$relation_fields r]~1~
	FROM rdb$relation_fields r
	INNER JOIN rdb$relation_constraints r_con
		ON r_con.rdb$relation_name = r.rdb$relation_name AND r_con.rdb$constraint_type = 'PRIMARY KEY'
	INNER JOIN rdb$index_segments r_seg 
		ON r_seg.rdb$index_name = r_con.rdb$index_name AND r_seg.rdb$field_name = r.rdb$field_name
	INNER JOIN rdb$triggers r_trg
		ON r_seg.rdb$field_name IS NOT NULL AND r_trg.rdb$relation_name = r_con.rdb$relation_name
			AND r_trg.rdb$trigger_sequence = 1 AND r_trg.rdb$flags = 1 AND r_trg.rdb$trigger_type = 1
	INNER JOIN rdb$dependencies r_dep
		ON r_dep.rdb$dependent_name = r_trg.rdb$trigger_name
			AND r_dep.rdb$depended_on_name = r_trg.rdb$relation_name AND r_dep.rdb$field_name = r.rdb$field_name
		WHERE r.rdb$relation_name = 'CRSTATUS'
	-- [crlfWHERE r.rdb$relation_name = '...']~2~ directly after ' = r.rdb$field_name'

	INTO :PRIMARY_DEPENDENCYCOUNT;

	IF (:PRIMARY_DEPENDENCYCOUNT IS NULL) THEN
		:PRIMARY_DEPENDENCYCOUNT = 0;

	FOR
		SELECT
			-- :TABLE_CATALOG, :TABLE_SCHEMA, :TABLE_NAME, :COLUMN_NAME, :COLUMN_SUB_TYPE
			null, null, r.rdb$relation_name, r.rdb$field_name, r_fld.rdb$field_sub_type,
			-- :COLUMN_SIZE
			CAST(r_fld.rdb$field_length AS integer),
			-- :NUMERIC_PRECISION
			CAST(r_fld.rdb$field_precision AS integer),
			-- :NUMERIC_SCALE
			CAST(r_fld.rdb$field_scale AS integer),
			-- CHARACTER_MAX_LENGTH
			CAST(r_fld.rdb$character_length AS integer),
			-- :CHARACTER_OCTET_LENGTH
			CAST(r_fld.rdb$field_length AS integer),
			-- ORDINAL_POSITION - [r.rdb$field_position]~3~
			r.rdb$field_position,
			-- :DOMAIN_CATALOG, :DOMAIN_SCHEMA, :DOMAIN_NAME
			null, null, r.rdb$field_source,
			-- :COLUMN_DEFAULT
			r.rdb$default_source,
			-- :EXPRESSION
			(CASE WHEN r_fld.rdb$computed_source IS NULL AND r_fld.rdb$computed_blr IS NOT NULL THEN
					cast(r_fld.rdb$computed_blr as blob sub_type 1)
			ELSE
					r_fld.rdb$computed_source
			END),
			-- :IS_COMPUTED
			(CASE WHEN r_fld.rdb$computed_source IS NULL AND r_fld.rdb$computed_blr IS NULL THEN
					false
			ELSE
					true
			END),
			-- :IS_ARRAY
			(CASE WHEN r_fld.rdb$dimensions IS NULL THEN false ELSE true END),
			-- :IS_NULLABLE
			(CASE WHEN coalesce(r_fld.rdb$null_flag, r.rdb$null_flag) IS NULL THEN true ELSE false END),
			-- :READONLY_FLAG
			0,
			-- :FIELD_TYPE
			r_fld.rdb$field_type,
			-- :CHARACTER_SET_CATALOG, :CHARACTER_SET_SCHEMA, :CHARACTER_SET_NAME
			null, null, r_cs.rdb$character_set_name,
			-- :COLLATION_CATALOG, :COLLATION_SCHEMA, :COLLATION_NAME
			null, null, r_coll.rdb$collation_name,
			-- :DESCRIPTION
			r.rdb$description,
			-- :IN_PRIMARYKEY
			(CASE WHEN r_seg.rdb$field_name IS NULL THEN false ELSE true END),
			-- :IS_IDENTITY
			(CASE WHEN r_dep.rdb$dependent_name IS NOT NULL AND r_trg.rdb$trigger_name IS NOT NULL AND r_trg.rdb$trigger_sequence = 1 AND r_trg.rdb$flags = 1 and r_trg.rdb$trigger_type = 1 THEN
				true
			ELSE
				false
			END),
			-- :SEGMENT_FIELD
			r_seg.rdb$field_name,
			-- :IDENTITY_TYPE - [r.rdb$identity_type|null]~4~
			r.rdb$identity_type,
			-- :SEQUENCE_GENERATOR, :IDENTITY_SEED, :IDENTITY_INCREMENT
			r_gen.rdb$generator_name, r_gen.rdb$initial_value, r_gen.rdb$generator_increment,
			-- :PARENT_TYPE - [Table|'ParentType']~5~
			-- [, r_dep.rdb$dependent_name]~6~ (for additional columns)
			'Table',
			r_dep.rdb$dependent_name
		-- [rdb$relation_fields r]~1~
		FROM rdb$relation_fields r
		INNER JOIN rdb$fields r_fld
			ON r_fld.rdb$field_name = r.rdb$field_source
		LEFT OUTER JOIN rdb$character_sets r_cs
			ON r_cs.rdb$character_set_id = r_fld.rdb$character_set_id
		LEFT OUTER JOIN rdb$collations r_coll
			ON (r_coll.rdb$collation_id = r_fld.rdb$collation_id AND r_coll.rdb$character_set_id = r_fld.rdb$character_set_id)
        LEFT OUTER JOIN rdb$relation_constraints r_con
			ON r_con.rdb$relation_name = r.rdb$relation_name AND r_con.rdb$constraint_type = 'PRIMARY KEY'
        LEFT OUTER JOIN rdb$index_segments r_seg 
            ON r_seg.rdb$index_name = r_con.rdb$index_name AND r_seg.rdb$field_name = r.rdb$field_name
        LEFT OUTER JOIN rdb$triggers r_trg
            ON r_trg.rdb$relation_name = r_con.rdb$relation_name AND r_trg.rdb$trigger_sequence = 1 AND r_trg.rdb$flags = 1 AND r_trg.rdb$trigger_type = 1
        LEFT OUTER JOIN rdb$dependencies r_dep
            ON r_dep.rdb$depended_on_name = r_trg.rdb$relation_name AND r_dep.rdb$field_name = r_seg.rdb$field_name
                AND r_dep.rdb$dependent_name = r_trg.rdb$trigger_name
        LEFT OUTER JOIN rdb$generators r_gen
			-- [= r.rdb$generator_name|IS NULL]~7~
		-- [crlfWHERE r.rdb$relation_name = '...']~2~
            ON r_gen.rdb$generator_name= r.rdb$generator_name
		WHERE r.rdb$relation_name = 'CRSTATUS'
		-- [r.rdb$relation_name]~8~ [r.rdb$field_position]~3~
		ORDER BY r.rdb$relation_name, r.rdb$field_position

        INTO :TABLE_CATALOG, :TABLE_SCHEMA, :TABLE_NAME, :COLUMN_NAME, :COLUMN_SUB_TYPE,
        :COLUMN_SIZE, :NUMERIC_PRECISION, :NUMERIC_SCALE, :CHARACTER_MAX_LENGTH, :CHARACTER_OCTET_LENGTH,
        :ORDINAL_POSITION, :DOMAIN_CATALOG, :DOMAIN_SCHEMA, :DOMAIN_NAME, :COLUMN_DEFAULT,
        :EXPRESSION, :IS_COMPUTED, :IS_ARRAY, :IS_NULLABLE, :READONLY_FLAG, :FIELD_TYPE,
        :CHARACTER_SET_CATALOG, :CHARACTER_SET_SCHEMA, :CHARACTER_SET_NAME, :COLLATION_CATALOG, :COLLATION_SCHEMA,
        :COLLATION_NAME, :DESCRIPTION,
		:IN_PRIMARYKEY, :IS_IDENTITY, :SEGMENT_FIELD,
		-- [,crlfTRIGGER_NAME]~9~ directly after :PARENT_TYPE
		:IDENTITY_TYPE, :SEQUENCE_GENERATOR, :IDENTITY_SEED, :IDENTITY_INCREMENT, :PARENT_TYPE,
			:TRIGGER_NAME
	DO BEGIN
		IF (:SEGMENT_FIELD IS NULL OR :PRIMARY_DEPENDENCYCOUNT <> 1) THEN
			:IS_IDENTITY = false;

		IF (:SEQUENCE_GENERATOR IS NOT NULL) THEN
		BEGIN
			EXECUTE STATEMENT 'SELECT gen_id(' || SEQUENCE_GENERATOR || ', 0) FROM rdb$database' INTO :IDENTITY_CURRENT;
			:IDENTITY_CURRENT = :IDENTITY_CURRENT - :IDENTITY_INCREMENT;
			IF (:IDENTITY_CURRENT < :IDENTITY_SEED) THEN
			BEGIN
				:IDENTITY_CURRENT = :IDENTITY_SEED;
			END

			IF (:IS_IDENTITY) THEN
			BEGIN
				-- There is a generator so :IDENTITY_TYPE determines if is-identity still holds true
				IF (:IDENTITY_TYPE IS NULL OR :IDENTITY_TYPE = 0) THEN
					:IS_IDENTITY = false;
			END
		END
        if (:IS_IDENTITY AND :SEQUENCE_GENERATOR IS NULL) THEN
		BEGIN
            -- [Unidentifiable]~10~
            :SEQUENCE_GENERATOR = 'Unidentifiable';
			:IDENTITY_SEED = -1;
			:IDENTITY_INCREMENT = -1;
			:IDENTITY_CURRENT = -1;
        END
		IF (NOT :IS_IDENTITY  AND :SEQUENCE_GENERATOR IS NULL) THEN
		BEGIN
			:SEQUENCE_GENERATOR = NULL;
			:IDENTITY_SEED = 0;
			:IDENTITY_INCREMENT = 0;
			:IDENTITY_CURRENT = 0;
		END

		IF (:IN_PRIMARYKEY AND :PRIMARY_DEPENDENCYCOUNT = 1) THEN
			:IS_UNIQUE = true;
		ELSE
			:IS_UNIQUE = false;
		

-- End section 2 

-- Begin section 3 - Column row complete

        SUSPEND;            

-- End section 3

-- Finalize

	END
END
^
SET TERM ; ^