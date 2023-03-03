SET TERM ^ ;
EXECUTE BLOCK
	RETURNS (
		GENERATOR_CATALOG varchar(50),
		GENERATOR_SCHEMA varchar(50),
		GENERATOR_NAME varchar(100),
		IS_SYSTEM_OBJECT boolean,
		GENERATOR_ID smallint,
		INITIAL_VALUE bigint,
		GENERATOR_INCREMENT int,
		NEXT_VALUE bigint)
AS
BEGIN
	FOR SELECT
		null, null, rdb$generator_name, (CASE WHEN rdb$system_flag <> 1 THEN false ELSE true END),
		rdb$generator_id, rdb$initial_value, rdb$generator_increment 
	FROM rdb$generators ORDER BY rdb$generator_name INTO :GENERATOR_CATALOG, :GENERATOR_SCHEMA, :GENERATOR_NAME, :IS_SYSTEM_OBJECT, :GENERATOR_ID, :INITIAL_VALUE, :GENERATOR_INCREMENT
	DO BEGIN
		EXECUTE STATEMENT 'SELECT gen_id(' || GENERATOR_NAME || ', 0) FROM rdb$database' INTO :NEXT_VALUE;
        SUSPEND;
    END
END
^
SET TERM ; ^