SET TERM ^ ;
EXECUTE BLOCK
	RETURNS (
        TABLE_CATALOG varchar(10), TABLE_SCHEMA varchar(10), TABLE_NAME varchar(50), COLUMN_NAME varchar(50),
		IS_SYSTEM_FLAG int, FIELD_SUB_TYPE smallint,
        FIELD_SIZE int, NUMERIC_PRECISION int, NUMERIC_SCALE int, CHARACTER_MAX_LENGTH int, CHARACTER_OCTET_LENGTH int,
        DOMAIN_CATALOG varchar(10), DOMAIN_SCHEMA varchar(1), DOMAIN_NAME varchar(50), FIELD_DEFAULT blob sub_type 1,
        EXPRESSION blob sub_type 1, IS_COMPUTED boolean, IS_ARRAY boolean, IS_NULLABLE boolean, READONLY_FLAG smallint, FIELD_TYPE smallint,
        CHARACTER_SET_CATALOG varchar(10), CHARACTER_SET_SCHEMA varchar(10), CHARACTER_SET_NAME varchar(50), 
        COLLATION_CATALOG varchar(10), COLLATION_SCHEMA varchar(10), COLLATION_NAME varchar(50),
        DESCRIPTION varchar(50), IN_PRIMARYKEY boolean, IS_UNIQUE boolean, IS_IDENTITY boolean, SEQUENCE_GENERATOR varchar(50),
        -- [returnClause]~0~ directly after TRIGGER_NAME varchar(50)
		IDENTITY_SEED bigint, IDENTITY_INCREMENT int, IDENTITY_CURRENT bigint, PARENT_TYPE varchar(15),
		ORDINAL_POSITION smallint, TRIGGER_NAME varchar(50),
		FUNCTION_CATALOG varchar(10),
		FUNCTION_SCHEMA varchar(10),
		FUNCTION_NAME varchar(50),
		ARGUMENT_NAME varchar(50),
		PSEUDO_NAME varchar(50),
		PACKAGE_NAME varchar(10))
AS
DECLARE PRIMARY_DEPENDENCYCOUNT int;
DECLARE TRIGGER_DEPENDENCYCOUNT int;
DECLARE IDENTITY_TYPE smallint;
DECLARE SEGMENT_FIELD varchar(50);
BEGIN
    :TRIGGER_DEPENDENCYCOUNT = 0;

	SELECT COUNT(*)
	-- [rdb$relation_fields r]~1~
	FROM rdb$function_arguments r
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
		WHERE r.rdb$function_name = 'MYFUNC'
	-- [crlfWHERE r.rdb$relation_name = '...']~2~ directly after ' = r.rdb$field_name'

	INTO :PRIMARY_DEPENDENCYCOUNT;

	IF (:PRIMARY_DEPENDENCYCOUNT IS NULL) THEN
		:PRIMARY_DEPENDENCYCOUNT = 0;

	FOR
		SELECT
			-- :TABLE_CATALOG, :TABLE_SCHEMA, :TABLE_NAME, :COLUMN_NAME
			null, null, r.rdb$relation_name, r.rdb$field_name,
			-- :IS_SYSTEM_FLAG,
			(CASE WHEN r.rdb$system_flag <> 1 THEN 0 ELSE 1 END),
			-- :FIELD_SUB_TYPE
			r_fld.rdb$field_sub_type,
			-- :FIELD_SIZE
			CAST(r_fld.rdb$field_length AS integer),
			-- :NUMERIC_PRECISION
			CAST(r_fld.rdb$field_precision AS integer),
			-- :NUMERIC_SCALE
			CAST(r_fld.rdb$field_scale AS integer),
			-- CHARACTER_MAX_LENGTH
			CAST(r_fld.rdb$character_length AS integer),
			-- :CHARACTER_OCTET_LENGTH
			CAST(r_fld.rdb$field_length AS integer),
			-- :DOMAIN_CATALOG, :DOMAIN_SCHEMA, :DOMAIN_NAME
			null, null, r_fld.rdb$field_name,
			-- :FIELD_DEFAULT
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
			-- :IDENTITY_TYPE - [r.rdb$identity_type|null]~3~
			null,
			-- :SEQUENCE_GENERATOR, :IDENTITY_SEED, :IDENTITY_INCREMENT
			r_gen.rdb$generator_name, r_gen.rdb$initial_value, r_gen.rdb$generator_increment,
			-- :PARENT_TYPE - [Table|'ParentType']~4~
			-- [, r_dep.rdb$dependent_name]~5~ (for additional columns)
			'Function',
			r.rdb$argument_position,
			r_dep.rdb$dependent_name,
			null,
			null,
			r.rdb$function_name,
			r.rdb$argument_name,
			(CASE WHEN r.rdb$argument_name IS NULL THEN '@RETURN_VALUE' ELSE r.rdb$argument_name END),
			(CASE WHEN r.rdb$system_flag <> 1 THEN 'USER' ELSE 'SYSTEM' END)
		-- [rdb$relation_fields r]~1~
		FROM rdb$function_arguments r
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
			-- [= r.rdb$generator_name|IS NULL]~6~
		-- [crlfWHERE r.rdb$relation_name = '...']~2~
            ON r_gen.rdb$generator_name IS NULL
		WHERE r.rdb$function_name = 'MYFUNC'
		-- [r.rdb$relation_name]~7~ [r.rdb$field_position]~8~
		ORDER BY r.rdb$function_name, r.rdb$argument_position

        INTO :TABLE_CATALOG, :TABLE_SCHEMA, :TABLE_NAME, :COLUMN_NAME, :IS_SYSTEM_FLAG,
		:FIELD_SUB_TYPE, :FIELD_SIZE, :NUMERIC_PRECISION, :NUMERIC_SCALE,
		:CHARACTER_MAX_LENGTH, :CHARACTER_OCTET_LENGTH,
        :DOMAIN_CATALOG, :DOMAIN_SCHEMA, :DOMAIN_NAME, :FIELD_DEFAULT,
        :EXPRESSION, :IS_COMPUTED, :IS_ARRAY, :IS_NULLABLE, :READONLY_FLAG, :FIELD_TYPE,
        :CHARACTER_SET_CATALOG, :CHARACTER_SET_SCHEMA, :CHARACTER_SET_NAME, :COLLATION_CATALOG, :COLLATION_SCHEMA,
        :COLLATION_NAME, :DESCRIPTION,
		:IN_PRIMARYKEY, :IS_IDENTITY, :SEGMENT_FIELD,
		-- [,crlfTRIGGER_NAME]~9~ directly after :PARENT_TYPE
		:IDENTITY_TYPE, :SEQUENCE_GENERATOR, :IDENTITY_SEED, :IDENTITY_INCREMENT, :PARENT_TYPE,
		:ORDINAL_POSITION,
		:TRIGGER_NAME,
			:FUNCTION_CATALOG,
			:FUNCTION_SCHEMA,
			:FUNCTION_NAME,
			:ARGUMENT_NAME,
			:PSEUDO_NAME,
			:PACKAGE_NAME
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