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
					coidxseg.rdb$field_position as ORDINAL_POSITION
				
				
				
				
				
				
				
				
				
				
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