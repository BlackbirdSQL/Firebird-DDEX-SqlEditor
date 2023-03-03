-- Begin section 1 - Main Select
-- Begin section 1 - Main Select

SELECT
	null AS CONSTRAINT_CATALOG,
	null AS CONSTRAINT_SCHEMA,
	co.rdb$constraint_name AS CONSTRAINT_NAME,
	null AS TABLE_CATALOG,
	null AS TABLE_SCHEMA,
	co.rdb$relation_name AS TABLE_NAME,
	null as REFERENCED_TABLE_CATALOG,
	null as REFERENCED_TABLE_SCHEMA,
	refidx.rdb$relation_name as REFERENCED_TABLE_NAME,
	refidx.rdb$index_name as REFERENCED_INDEX_NAME,
	co.rdb$deferrable AS IS_DEFERRABLE,
	co.rdb$initially_deferred AS INITIALLY_DEFERRED,
	ref.rdb$match_option AS MATCH_OPTION,
	CASE (ref.rdb$update_rule)
        WHEN 'CASCADE' THEN 1
        WHEN 'SET NULL' THEN 2
        WHEN 'SET_DEFAULT' THEN 3
        ELSE 0
    END AS UPDATE_ACTION,
	CASE (ref.rdb$delete_rule)
        WHEN 'CASCADE' THEN 1
        WHEN 'SET NULL' THEN 2
        WHEN 'SET_DEFAULT' THEN 3
        ELSE 0
    END AS DELETE_ACTION,
	co.rdb$index_name as INDEX_NAME
FROM rdb$relation_constraints co
INNER JOIN rdb$ref_constraints ref
	ON co.rdb$constraint_name = ref.rdb$constraint_name
INNER JOIN rdb$indices tempidx
	ON co.rdb$index_name = tempidx.rdb$index_name
INNER JOIN rdb$indices refidx
	ON refidx.rdb$index_name = tempidx.rdb$foreign_key		
-- End of section 1

-- Begin section 2 - Where clause

WHERE co.rdb$constraint_type = 'FOREIGN KEY'

-- End section 2 

-- Begin section 3 - Order

 ORDER BY TABLE_NAME, CONSTRAINT_NAME

-- End section 3

-- Finalize
