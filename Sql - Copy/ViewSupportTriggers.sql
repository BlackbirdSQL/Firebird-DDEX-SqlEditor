SELECT
					null AS TABLE_CATALOG,
					null AS TABLE_SCHEMA,
					rfr.rdb$relation_name AS TABLE_NAME,
					rfr.rdb$trigger_name AS TRIGGER_NAME,
					rfr.rdb$system_flag AS IS_SYSTEM_TRIGGER,
					rfr.rdb$trigger_type AS TRIGGER_TYPE,
					rfr.rdb$trigger_inactive AS IS_INACTIVE,
					rfr.rdb$trigger_sequence AS SEQUENCENO,
					(CASE WHEN rfr.rdb$trigger_source IS NULL AND rfr.rdb$trigger_blr IS NOT NULL THEN
                        cast(rfr.rdb$trigger_blr as blob sub_type 1)
					ELSE
                        rfr.rdb$trigger_source
					END) AS EXPRESSION,
					(rfr.rdb$description) AS DESCRIPTION,
					(CASE WHEN rfr.rdb$flags = 1 and rfr.rdb$system_flag = 0 and rfr.rdb$trigger_type = 1 THEN
						TRUE
					ELSE
						FALSE
					END) AS IS_AUTOINCREMENT,
					LIST(TRIM(dep.rdb$field_name), ', ') AS DEPENDENT_FIELDS
				FROM rdb$triggers rfr
				LEFT OUTER JOIN rdb$dependencies dep
					ON dep.rdb$depended_on_name = rfr.rdb$relation_name AND dep.rdb$dependent_name = rfr.rdb$trigger_name
                WHERE rfr.rdb$relation_name = 'STOCK_SUPPLIER'
                GROUP BY TABLE_CATALOG, TABLE_SCHEMA, TABLE_NAME, TRIGGER_NAME, IS_SYSTEM_TRIGGER, TRIGGER_TYPE, IS_INACTIVE, SEQUENCENO, EXPRESSION, DESCRIPTION, IS_AUTOINCREMENT
                ORDER BY TRIGGER_NAME