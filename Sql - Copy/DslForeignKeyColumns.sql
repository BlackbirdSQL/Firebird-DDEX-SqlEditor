SELECT
					null AS CONSTRAINT_CATALOG,
					null AS CONSTRAINT_SCHEMA,
					co.rdb$constraint_name AS CONSTRAINT_NAME,
					null AS TABLE_CATALOG,
					null AS TABLE_SCHEMA,
					co.rdb$relation_name AS TABLE_NAME,
					coidxseg.rdb$field_name AS COLUMN_NAME,
					null as REFERENCED_TABLE_CATALOG,
					null as REFERENCED_TABLE_SCHEMA,
					refidx.rdb$relation_name as REFERENCED_TABLE_NAME,
					refidxseg.rdb$field_name AS REFERENCED_COLUMN_NAME,
					
					
					dep.rdb$dependent_name AS TRIGGER_NAME,
				    null AS COLUMN_DATA_TYPE,
				    fld.rdb$field_sub_type AS COLUMN_SUB_TYPE,
					CAST(fld.rdb$field_length AS integer) AS COLUMN_SIZE,
					CAST(fld.rdb$field_precision AS integer) AS NUMERIC_PRECISION,
					CAST(fld.rdb$field_scale AS integer) AS NUMERIC_SCALE,
					CAST(fld.rdb$character_length AS integer) AS CHARACTER_MAX_LENGTH,
					CAST(fld.rdb$field_length AS integer) AS CHARACTER_OCTET_LENGTH,
					coidxseg.rdb$field_position as ORDINAL_POSITION,
					null AS DOMAIN_CATALOG,
					null AS DOMAIN_SCHEMA,
					rfr.rdb$field_source AS DOMAIN_NAME,
					null AS SYSTEM_DATA_TYPE,
					rfr.rdb$default_source AS COLUMN_DEFAULT,
					(CASE WHEN fld.rdb$computed_source IS NULL AND fld.rdb$computed_blr IS NOT NULL THEN
						 cast(fld.rdb$computed_blr as blob sub_type 1)
					ELSE
						 fld.rdb$computed_source
					END) AS EXPRESSION,
					(CASE WHEN fld.rdb$computed_source IS NULL AND fld.rdb$computed_blr IS NULL THEN
						 FALSE
					ELSE
						 TRUE
					END) AS IS_COMPUTED,
					fld.rdb$dimensions AS COLUMN_ARRAY,
					coalesce(fld.rdb$null_flag, rfr.rdb$null_flag) AS COLUMN_NULLABLE,
				    0 AS IS_READONLY,
					fld.rdb$field_type AS FIELD_TYPE,
					null AS CHARACTER_SET_CATALOG,
					null AS CHARACTER_SET_SCHEMA,
					cs.rdb$character_set_name AS CHARACTER_SET_NAME,
					null AS COLLATION_CATALOG,
					null AS COLLATION_SCHEMA,
					coll.rdb$collation_name AS COLLATION_NAME,
					rfr.rdb$description AS DESCRIPTION,
					0 AS IDENTITY_TYPE,
					(CASE WHEN seg.rdb$field_name IS NULL THEN
						FALSE
					ELSE
						TRUE
					END) AS IN_PRIMARYKEY,
					(CASE WHEN dep.rdb$dependent_name IS NOT NULL AND trg.rdb$trigger_name IS NOT NULL AND trg.rdb$trigger_sequence = 1 AND trg.rdb$flags = 1 and trg.rdb$trigger_type = 1 THEN
						1
					ELSE
						0
					END) AS IS_AUTOINCREMENT,
					(SELECT COUNT(*)
                        FROM rdb$dependencies fd
                        WHERE fd.rdb$field_name IS NOT NULL AND fd.rdb$dependent_name = trg.rdb$trigger_name AND fd.rdb$depended_on_name = trg.rdb$relation_name
						GROUP BY fd.rdb$dependent_name, fd.rdb$depended_on_name)
                    AS TRIGGER_DEPENDENCYCOUNT
				FROM rdb$relation_constraints co
				INNER JOIN rdb$ref_constraints ref 
                    ON co.rdb$constraint_name = ref.rdb$constraint_name
				INNER JOIN rdb$indices tempidx 
                    ON co.rdb$index_name = tempidx.rdb$index_name
				INNER JOIN rdb$index_segments coidxseg 
                    ON co.rdb$index_name = coidxseg.rdb$index_name
				INNER JOIN rdb$indices refidx 
                    ON refidx.rdb$index_name = tempidx.rdb$foreign_key
				INNER JOIN rdb$index_segments refidxseg 
                    ON refidxseg.rdb$index_name = refidx.rdb$index_name AND refidxseg.rdb$field_position = coidxseg.rdb$field_position
				INNER JOIN rdb$relation_fields rfr
                    ON rfr.rdb$relation_name = co.rdb$relation_name AND rfr.rdb$field_name = coidxseg.rdb$field_name
				INNER JOIN rdb$fields fld
					ON rfr.rdb$field_source = fld.rdb$field_name
				LEFT JOIN rdb$character_sets cs
					ON cs.rdb$character_set_id = fld.rdb$character_set_id
				LEFT JOIN rdb$collations coll
					ON (coll.rdb$collation_id = fld.rdb$collation_id AND coll.rdb$character_set_id = fld.rdb$character_set_id)
				LEFT JOIN rdb$relation_constraints con
					ON con.rdb$relation_name = rfr.rdb$relation_name AND con.rdb$constraint_type = 'PRIMARY KEY'
				LEFT JOIN rdb$index_segments seg 
					ON seg.rdb$index_name = con.rdb$index_name AND seg.rdb$field_name = rfr.rdb$field_name
                LEFT JOIN rdb$triggers trg
                    ON trg.rdb$relation_name = con.rdb$relation_name AND trg.rdb$trigger_sequence = 1 AND trg.rdb$flags = 1 and trg.rdb$trigger_type = 1
                        AND seg.rdb$index_name = con.rdb$index_name AND seg.rdb$field_name = rfr.rdb$field_name
				LEFT JOIN rdb$dependencies dep
					ON dep.rdb$field_name IS NOT NULL AND dep.rdb$depended_on_name = trg.rdb$relation_name AND dep.rdb$dependent_name = trg.rdb$trigger_name AND dep.rdb$field_name = seg.rdb$field_name