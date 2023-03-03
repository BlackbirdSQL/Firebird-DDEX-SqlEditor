SELECT
					null AS VIEW_CATALOG,
					null AS VIEW_SCHEMA,
					rel.rdb$relation_name AS VIEW_NAME,
					rfr.rdb$field_name AS COLUMN_NAME,
					null AS COLUMN_DATA_TYPE,
					fld.rdb$field_sub_type AS COLUMN_SUB_TYPE,
					CAST(fld.rdb$field_length AS integer) AS COLUMN_SIZE,
					CAST(fld.rdb$field_precision AS integer) AS NUMERIC_PRECISION,
					CAST(fld.rdb$field_scale AS integer) AS NUMERIC_SCALE,
					CAST(fld.rdb$character_length AS integer) AS CHARACTER_MAX_LENGTH,
					CAST(fld.rdb$field_length AS integer) AS CHARACTER_OCTET_LENGTH,
					rfr.rdb$field_position AS ORDINAL_POSITION,
					fld.rdb$default_source AS COLUMN_DEFAULT,
					coalesce(fld.rdb$null_flag, rfr.rdb$null_flag) AS COLUMN_NULLABLE,
					fld.rdb$dimensions AS COLUMN_ARRAY,
					0 AS IS_READONLY,
					fld.rdb$field_type AS FIELD_TYPE,
					null AS CHARACTER_SET_CATALOG,
					null AS CHARACTER_SET_SCHEMA,
					cs.rdb$character_set_name AS CHARACTER_SET_NAME,
					null AS COLLATION_CATALOG,
					null AS COLLATION_SCHEMA,
					coll.rdb$collation_name AS COLLATION_NAME,
					rfr.rdb$description AS DESCRIPTION,
					(CASE WHEN seg.rdb$field_name IS NULL THEN
						FALSE
					ELSE
						TRUE
					END) AS IN_PRIMARYKEY,
				    fld.rdb$computed_source AS EXPRESSION,
					0 AS IDENTITY_TYPE
				FROM rdb$relations rel
					LEFT OUTER JOIN rdb$relation_fields rfr ON rel.rdb$relation_name = rfr.rdb$relation_name
					LEFT OUTER JOIN rdb$fields fld ON rfr.rdb$field_source = fld.rdb$field_name
					LEFT OUTER JOIN rdb$character_sets cs ON cs.rdb$character_set_id = fld.rdb$character_set_id
				    LEFT OUTER JOIN rdb$collations coll ON (coll.rdb$collation_id = fld.rdb$collation_id AND coll.rdb$character_set_id = fld.rdb$character_set_id)
					LEFT OUTER JOIN rdb$relation_constraints con
						ON con.rdb$relation_name = rfr.rdb$relation_name AND con.rdb$constraint_type = 'PRIMARY KEY'
					LEFT OUTER JOIN rdb$index_segments seg 
						ON seg.rdb$index_name = con.rdb$index_name AND seg.rdb$field_name = rfr.rdb$field_name