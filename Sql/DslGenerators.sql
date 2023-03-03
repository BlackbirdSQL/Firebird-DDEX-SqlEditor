SET TERM ^ ;

-- Select section

EXECUTE BLOCK
	RETURNS (
		GENERATOR_CATALOG varchar(50),
		GENERATOR_SCHEMA varchar(50),
		GENERATOR_NAME varchar(100),
		IS_SYSTEM_OBJECT boolean,
		GENERATOR_ID smallint,
		GENERATOR_IDENTITY int,
		IDENTITY_SEED bigint,
		IDENTITY_INCREMENT int,
		IDENTITY_CURRENT bigint)
AS
BEGIN
	FOR SELECT
		null, null, rdb$generator_name, (CASE WHEN rdb$system_flag <> 1 THEN false ELSE true END),
		rdb$generator_id, rdb$initial_value, rdb$generator_increment 
	FROM rdb$generators

-- End 	Select section


-- Begin section 2 - Order by

	ORDER BY rdb$generator_name
	
-- End section 2


-- Begin section 3 - Dependency select

	INTO :GENERATOR_CATALOG, :GENERATOR_SCHEMA, :GENERATOR_NAME, :IS_SYSTEM_OBJECT, :GENERATOR_ID, :IDENTITY_SEED, :IDENTITY_INCREMENT
	DO BEGIN
		EXECUTE STATEMENT 'SELECT gen_id(' || GENERATOR_NAME || ', 0) FROM rdb$database' INTO :IDENTITY_CURRENT;
		:IDENTITY_CURRENT = :IDENTITY_CURRENT - :IDENTITY_INCREMENT;
		IF (:IDENTITY_CURRENT < :IDENTITY_SEED) THEN
		BEGIN
			:IDENTITY_CURRENT = :IDENTITY_SEED;
		END
        SUSPEND;
    END
END
^
SET TERM ; ^