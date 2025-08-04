/*
 *	This is a replication of the FirebirdClient Class which is not accessible
 *	
 *    The contents of this file are subject to the Initial
 *    Developer's Public License Version 1.0 (the "License");
 *    you may not use this file except in compliance with the
 *    License. You may obtain a copy of the License at
 *    https://github.com/FirebirdSQL/NETProvider/raw/master/license.txt.
 *
 *    Software distributed under the License is distributed on
 *    an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either
 *    express or implied. See the License for the specific
 *    language governing rights and limitations under the License.
 *
 *    All Rights Reserved.
 */

//$OriginalAuthors = Carlos Guzman Alvarez, Jiri Cincura (jiri@cincura.net)

// This file was originally ported from Jaybird



namespace BlackbirdSql.Data.Model;


internal static class IscCodes
{
	#region General

	internal const int SQLDA_VERSION1 = 1;
	internal const int SQL_DIALECT_V5 = 1;
	internal const int SQL_DIALECT_V6_TRANSITION = 2;
	internal const int SQL_DIALECT_V6 = 3;
	internal const int SQL_DIALECT_CURRENT = SQL_DIALECT_V6;
	internal const int DSQL_close = 1;
	internal const int DSQL_drop = 2;
	internal const int ARRAY_DESC_COLUMN_MAJOR = 1;   /* Set for FORTRAN */
	internal const int ISC_STATUS_LENGTH = 20;
	internal const ushort INVALID_OBJECT = 0xFFFF;

	#endregion

	#region Buffer sizes

	internal const int BUFFER_SIZE_128 = 128;
	internal const int BUFFER_SIZE_256 = 256;
	internal const int BUFFER_SIZE_32K = 32768;
	internal const int DEFAULT_MAX_BUFFER_SIZE = 8192;
	internal const int ROWS_AFFECTED_BUFFER_SIZE = 34;
	internal const int STATEMENT_TYPE_BUFFER_SIZE = 8;
	internal const int PREPARE_INFO_BUFFER_SIZE = 32768;

	#endregion

	#region Protocol Codes

	internal const int GenericAchitectureClient = 1;

	internal const int CONNECT_VERSION2 = 2;
	internal const int CONNECT_VERSION3 = 3;
	internal const int PROTOCOL_VERSION3 = 3;
	internal const int PROTOCOL_VERSION4 = 4;
	internal const int PROTOCOL_VERSION5 = 5;
	internal const int PROTOCOL_VERSION6 = 6;
	internal const int PROTOCOL_VERSION7 = 7;
	internal const int PROTOCOL_VERSION8 = 8;
	internal const int PROTOCOL_VERSION9 = 9;
	internal const int PROTOCOL_VERSION10 = 10;

	internal const int FB_PROTOCOL_FLAG = 0x8000;
	internal const int FB_PROTOCOL_MASK = ~FB_PROTOCOL_FLAG;

	internal const int PROTOCOL_VERSION11 = FB_PROTOCOL_FLAG | 11;
	internal const int PROTOCOL_VERSION12 = FB_PROTOCOL_FLAG | 12;
	internal const int PROTOCOL_VERSION13 = FB_PROTOCOL_FLAG | 13;
	internal const int PROTOCOL_VERSION15 = FB_PROTOCOL_FLAG | 15;
	internal const int PROTOCOL_VERSION16 = FB_PROTOCOL_FLAG | 16;

	internal const int p_cnct_min_type = 0;

	internal const int ptype_rpc = 2;
	internal const int ptype_batch_send = 3;
	internal const int ptype_out_of_band = 4;
	internal const int ptype_lazy_send = 5;

	internal const int pflag_compress = 0x100;

	internal const int WIRE_CRYPT_DISABLED = 0;
	internal const int WIRE_CRYPT_ENABLED = 1;
	internal const int WIRE_CRYPT_REQUIRED = 2;

	#endregion

	#region Statement Flags

	internal const int STMT_DEFER_EXECUTE = 4;

	#endregion

	#region Server Class

	internal const int isc_info_db_class_classic_access = 13;
	internal const int isc_info_db_class_server_access = 14;

	#endregion

	#region Operation Codes

	// Operation (packet) types
	internal const int op_void = 0;   // Packet has been voided
	internal const int op_connect = 1;    // Connect to remote server
	internal const int op_exit = 2;   // Remote end has exitted
	internal const int op_accept = 3; // Server accepts connection
	internal const int op_reject = 4; // Server rejects connection
	internal const int op_protocol = 5;   // Protocol	selection
	internal const int op_disconnect = 6; // Connect is going	away
	internal const int op_credit = 7; // Grant (buffer) credits
	internal const int op_continuation = 8;   // Continuation	packet
	internal const int op_response = 9;   // Generic response	block

	// Page	server operations
	internal const int op_open_file = 10; // Open	file for page service
	internal const int op_create_file = 11;   // Create file for page	service
	internal const int op_close_file = 12;    // Close file for page service
	internal const int op_read_page = 13; // optionally lock and read	page
	internal const int op_write_page = 14;    // write page and optionally release lock
	internal const int op_lock = 15;  // sieze lock
	internal const int op_convert_lock = 16;  // convert existing	lock
	internal const int op_release_lock = 17;  // release existing	lock
	internal const int op_blocking = 18;  // blocking	lock message

	// Full	context	server operations
	internal const int op_attach = 19;    // Attach database
	internal const int op_create = 20;    // Create database
	internal const int op_detach = 21;    // Detach database
	internal const int op_compile = 22;   // Request based operations
	internal const int op_start = 23;
	internal const int op_start_and_send = 24;
	internal const int op_send = 25;
	internal const int op_receive = 26;
	internal const int op_unwind = 27;
	internal const int op_release = 28;

	internal const int op_transaction = 29;   // Transaction operations
	internal const int op_commit = 30;
	internal const int op_rollback = 31;
	internal const int op_prepare = 32;
	internal const int op_reconnect = 33;

	internal const int op_create_blob = 34;   // Blob	operations //
	internal const int op_open_blob = 35;
	internal const int op_get_segment = 36;
	internal const int op_put_segment = 37;
	internal const int op_cancel_blob = 38;
	internal const int op_close_blob = 39;

	internal const int op_info_database = 40; // Information services
	internal const int op_info_request = 41;
	internal const int op_info_transaction = 42;
	internal const int op_info_blob = 43;

	internal const int op_batch_segments = 44;    // Put a bunch of blob segments

	internal const int op_mgr_set_affinity = 45;  // Establish server	affinity
	internal const int op_mgr_clear_affinity = 46;    // Break server	affinity
	internal const int op_mgr_report = 47;    // Report on server

	internal const int op_que_events = 48;    // Que event notification request
	internal const int op_cancel_events = 49; // Cancel event	notification request
	internal const int op_commit_retaining = 50;  // Commit retaining	(what else)
	internal const int op_prepare2 = 51;  // Message form	of prepare
	internal const int op_event = 52; // Completed event request (asynchronous)
	internal const int op_connect_request = 53;   // Request to establish	connection
	internal const int op_aux_connect = 54;   // Establish auxilliary connection
	internal const int op_ddl = 55;   // DDL call
	internal const int op_open_blob2 = 56;
	internal const int op_create_blob2 = 57;
	internal const int op_get_slice = 58;
	internal const int op_put_slice = 59;
	internal const int op_slice = 60; // Successful response to public const int op_get_slice
	internal const int op_seek_blob = 61; // Blob	seek operation

	// DSQL	operations //
	internal const int op_allocate_statement = 62;    // allocate	a statment handle
	internal const int op_execute = 63;   // execute a prepared statement
	internal const int op_exec_immediate = 64;    // execute a statement
	internal const int op_fetch = 65; // fetch a record
	internal const int op_fetch_response = 66;    // response	for	record fetch
	internal const int op_free_statement = 67;    // free	a statement
	internal const int op_prepare_statement = 68; // prepare a statement
	internal const int op_set_cursor = 69;    // set a cursor	name
	internal const int op_info_sql = 70;
	internal const int op_dummy = 71; // dummy packet	to detect loss of client
	internal const int op_response_piggyback = 72;    // response	block for piggybacked messages
	internal const int op_start_and_receive = 73;
	internal const int op_start_send_and_receive = 74;

	internal const int op_exec_immediate2 = 75;   // execute an immediate	statement with msgs
	internal const int op_execute2 = 76;  // execute a statement with	msgs
	internal const int op_insert = 77;
	internal const int op_sql_response = 78;  // response	from execute; exec immed; insert
	internal const int op_transact = 79;
	internal const int op_transact_response = 80;
	internal const int op_drop_database = 81;
	internal const int op_service_attach = 82;
	internal const int op_service_detach = 83;
	internal const int op_service_info = 84;
	internal const int op_service_start = 85;
	internal const int op_rollback_retaining = 86;

	// Two following opcode are used in vulcan.
	// No plans to implement them completely for a while, but to
	// support protocol 11, where they are used, have them here.
	internal const int op_update_account_info = 87;
	internal const int op_authenticate_user = 88;

	internal const int op_partial = 89;   // packet is not complete - delay processing
	internal const int op_trusted_auth = 90;
	internal const int op_cancel = 91;
	internal const int op_cont_auth = 92;
	internal const int op_ping = 93;
	internal const int op_accept_data = 94;
	internal const int op_abort_aux_connection = 95;
	internal const int op_crypt = 96;
	internal const int op_crypt_key_callback = 97;
	internal const int op_cond_accept = 98;

	internal const int op_batch_create = 99;
	internal const int op_batch_msg = 100;
	internal const int op_batch_exec = 101;
	internal const int op_batch_rls = 102;
	internal const int op_batch_cs = 103;
	internal const int op_batch_regblob = 104;
	internal const int op_batch_blob_stream = 105;
	internal const int op_batch_set_bpb = 106;

	internal const int op_repl_data = 107;
	internal const int op_repl_req = 108;

	internal const int op_batch_cancel = 109;

	#endregion

	#region Database Parameter Block

	internal const int isc_dpb_version1 = 1;
	internal const int isc_dpb_version2 = 2;

	internal const int isc_dpb_cdd_pathname = 1;
	internal const int isc_dpb_allocation = 2;
	internal const int isc_dpb_journal = 3;
	internal const int isc_dpb_page_size = 4;
	internal const int isc_dpb_num_buffers = 5;
	internal const int isc_dpb_buffer_length = 6;
	internal const int isc_dpb_debug = 7;
	internal const int isc_dpb_garbage_collect = 8;
	internal const int isc_dpb_verify = 9;
	internal const int isc_dpb_sweep = 10;
	internal const int isc_dpb_enable_journal = 11;
	internal const int isc_dpb_disable_journal = 12;
	internal const int isc_dpb_dbkey_scope = 13;
	internal const int isc_dpb_number_of_users = 14;
	internal const int isc_dpb_trace = 15;
	internal const int isc_dpb_no_garbage_collect = 16;
	internal const int isc_dpb_damaged = 17;
	internal const int isc_dpb_license = 18;
	internal const int isc_dpb_sys_user_name = 19;
	internal const int isc_dpb_encrypt_key = 20;
	internal const int isc_dpb_activate_shadow = 21;
	internal const int isc_dpb_sweep_interval = 22;
	internal const int isc_dpb_delete_shadow = 23;
	internal const int isc_dpb_force_write = 24;
	internal const int isc_dpb_begin_log = 25;
	internal const int isc_dpb_quit_log = 26;
	internal const int isc_dpb_no_reserve = 27;
	internal const int isc_dpb_user_name = 28;
	internal const int isc_dpb_password = 29;
	internal const int isc_dpb_password_enc = 30;
	internal const int isc_dpb_sys_user_name_enc = 31;
	internal const int isc_dpb_interp = 32;
	internal const int isc_dpb_online_dump = 33;
	internal const int isc_dpb_old_file_size = 34;
	internal const int isc_dpb_old_num_files = 35;
	internal const int isc_dpb_old_file = 36;
	internal const int isc_dpb_old_start_page = 37;
	internal const int isc_dpb_old_start_seqno = 38;
	internal const int isc_dpb_old_start_file = 39;
	internal const int isc_dpb_drop_walfile = 40;
	internal const int isc_dpb_old_dump_id = 41;
	internal const int isc_dpb_wal_backup_dir = 42;
	internal const int isc_dpb_wal_chkptlen = 43;
	internal const int isc_dpb_wal_numbufs = 44;
	internal const int isc_dpb_wal_bufsize = 45;
	internal const int isc_dpb_wal_grp_cmt_wait = 46;
	internal const int isc_dpb_lc_messages = 47;
	internal const int isc_dpb_lc_ctype = 48;
	internal const int isc_dpb_cache_manager = 49;
	internal const int isc_dpb_shutdown = 50;
	internal const int isc_dpb_online = 51;
	internal const int isc_dpb_shutdown_delay = 52;
	internal const int isc_dpb_reserved = 53;
	internal const int isc_dpb_overwrite = 54;
	internal const int isc_dpb_sec_attach = 55;
	internal const int isc_dpb_disable_wal = 56;
	internal const int isc_dpb_connect_timeout = 57;
	internal const int isc_dpb_dummy_packet_interval = 58;
	internal const int isc_dpb_gbak_attach = 59;
	internal const int isc_dpb_sql_role_name = 60;
	internal const int isc_dpb_set_page_buffers = 61;
	internal const int isc_dpb_working_directory = 62;
	internal const int isc_dpb_sql_dialect = 63;
	internal const int isc_dpb_set_db_readonly = 64;
	internal const int isc_dpb_set_db_sql_dialect = 65;
	internal const int isc_dpb_gfix_attach = 66;
	internal const int isc_dpb_gstat_attach = 67;
	internal const int isc_dpb_set_db_charset = 68;
	internal const int isc_dpb_gsec_attach = 69;
	internal const int isc_dpb_address_path = 70;
	internal const int isc_dpb_process_id = 71;
	internal const int isc_dpb_no_db_triggers = 72;
	internal const int isc_dpb_trusted_auth = 73;
	internal const int isc_dpb_process_name = 74;
	internal const int isc_dpb_trusted_role = 75;
	internal const int isc_dpb_org_filename = 76;
	internal const int isc_dpb_utf8_filename = 77;
	internal const int isc_dpb_ext_call_depth = 78;
	internal const int isc_dpb_auth_block = 79;
	internal const int isc_dpb_client_version = 80;
	internal const int isc_dpb_remote_protocol = 81;
	internal const int isc_dpb_host_name = 82;
	internal const int isc_dpb_os_user = 83;
	internal const int isc_dpb_specific_auth_data = 84;
	internal const int isc_dpb_auth_plugin_list = 85;
	internal const int isc_dpb_auth_plugin_name = 86;
	internal const int isc_dpb_config = 87;
	internal const int isc_dpb_nolinger = 88;
	internal const int isc_dpb_reset_icu = 89;
	internal const int isc_dpb_map_attach = 90;
	internal const int isc_dpb_session_time_zone = 91;
	internal const int isc_dpb_set_db_replica = 92;
	internal const int isc_dpb_set_bind = 93;
	internal const int isc_dpb_decfloat_round = 94;
	internal const int isc_dpb_decfloat_traps = 95;
	internal const int isc_dpb_clear_map = 96;
	internal const int isc_dpb_parallel_workers = 100;
	internal const int isc_dpb_worker_attach = 101;

	#endregion

	#region Transaction Parameter Block

	internal const int isc_tpb_version1 = 1;
	internal const int isc_tpb_version3 = 3;
	internal const int isc_tpb_consistency = 1;
	internal const int isc_tpb_concurrency = 2;
	internal const int isc_tpb_shared = 3;
	internal const int isc_tpb_protected = 4;
	internal const int isc_tpb_exclusive = 5;
	internal const int isc_tpb_wait = 6;
	internal const int isc_tpb_nowait = 7;
	internal const int isc_tpb_read = 8;
	internal const int isc_tpb_write = 9;
	internal const int isc_tpb_lock_read = 10;
	internal const int isc_tpb_lock_write = 11;
	internal const int isc_tpb_verb_time = 12;
	internal const int isc_tpb_commit_time = 13;
	internal const int isc_tpb_ignore_limbo = 14;
	internal const int isc_tpb_read_committed = 15;
	internal const int isc_tpb_autocommit = 16;
	internal const int isc_tpb_rec_version = 17;
	internal const int isc_tpb_no_rec_version = 18;
	internal const int isc_tpb_restart_requests = 19;
	internal const int isc_tpb_no_auto_undo = 20;
	internal const int isc_tpb_lock_timeout = 21;
	internal const int isc_tpb_read_consistency = 22;
	internal const int isc_tpb_at_snapshot_number = 23;

	#endregion

	#region Services Parameter Block

	internal const int isc_spb_version1 = 1;
	internal const int isc_spb_current_version = 2;
	internal const int isc_spb_version = isc_spb_current_version;
	internal const int isc_spb_version3 = 3;
	internal const int isc_spb_user_name = isc_dpb_user_name;
	internal const int isc_spb_sys_user_name = isc_dpb_sys_user_name;
	internal const int isc_spb_sys_user_name_enc = isc_dpb_sys_user_name_enc;
	internal const int isc_spb_password = isc_dpb_password;
	internal const int isc_spb_password_enc = isc_dpb_password_enc;
	internal const int isc_spb_command_line = 105;
	internal const int isc_spb_dbname = 106;
	internal const int isc_spb_verbose = 107;
	internal const int isc_spb_options = 108;
	internal const int isc_spb_address_path = 109;
	internal const int isc_spb_process_id = 110;
	internal const int isc_spb_trusted_auth = 111;
	internal const int isc_spb_process_name = 112;
	internal const int isc_spb_trusted_role = 113;
	internal const int isc_spb_verbint = 114;
	internal const int isc_spb_auth_block = 115;
	internal const int isc_spb_auth_plugin_name = 116;
	internal const int isc_spb_auth_plugin_list = 117;
	internal const int isc_spb_utf8_filename = 118;
	internal const int isc_spb_client_version = 119;
	internal const int isc_spb_remote_protocol = 120;
	internal const int isc_spb_host_name = 121;
	internal const int isc_spb_os_user = 122;
	internal const int isc_spb_config = 123;
	internal const int isc_spb_expected_db = 124;

	internal const int isc_spb_connect_timeout = isc_dpb_connect_timeout;
	internal const int isc_spb_dummy_packet_interval = isc_dpb_dummy_packet_interval;
	internal const int isc_spb_sql_role_name = isc_dpb_sql_role_name;

	internal const int isc_spb_specific_auth_data = isc_spb_trusted_auth;

	internal const int isc_spb_num_att = 5;
	internal const int isc_spb_num_db = 6;

	#endregion

	#region Services Actions

	internal const int isc_action_svc_backup = 1; /* Starts database backup process on the server */
	internal const int isc_action_svc_restore = 2; /* Starts database restore process on the server */
	internal const int isc_action_svc_repair = 3; /* Starts database repair process on the server */
	internal const int isc_action_svc_add_user = 4; /* Adds a new user to the security database */
	internal const int isc_action_svc_delete_user = 5; /* Deletes a user record from the security database */
	internal const int isc_action_svc_modify_user = 6; /* Modifies a user record in the security database */
	internal const int isc_action_svc_display_user = 7; /* Displays a user record from the security database */
	internal const int isc_action_svc_properties = 8; /* Sets database properties */
	internal const int isc_action_svc_add_license = 9; /* Adds a license to the license file */
	internal const int isc_action_svc_remove_license = 10; /* Removes a license from the license file */
	internal const int isc_action_svc_db_stats = 11; /* Retrieves database statistics */
	internal const int isc_action_svc_get_ib_log = 12; /* Retrieves the InterBase log file from the server */
	internal const int isc_action_svc_get_fb_log = 12; /* Retrieves the Firebird log file from the server */
	internal const int isc_action_svc_nbak = 20; /* Incremental nbackup */
	internal const int isc_action_svc_nrest = 21; /* Incremental database restore */
	internal const int isc_action_svc_trace_start = 22; // Start trace session
	internal const int isc_action_svc_trace_stop = 23; // Stop trace session
	internal const int isc_action_svc_trace_suspend = 24; // Suspend trace session
	internal const int isc_action_svc_trace_resume = 25; // Resume trace session
	internal const int isc_action_svc_trace_list = 26; // List existing sessions
	internal const int isc_action_svc_set_mapping = 27; // Set auto admins mapping in security database
	internal const int isc_action_svc_drop_mapping = 28; // Drop auto admins mapping in security database
	internal const int isc_action_svc_display_user_adm = 29; // Displays user(s) from security database with admin info
	internal const int isc_action_svc_validate = 30; // Starts database online validation
	internal const int isc_action_svc_nfix = 31; // Fixup database after file system copy

	#endregion

	#region Services Information

	internal const int isc_info_svc_svr_db_info = 50; /* Retrieves the number	of attachments and databases */
	internal const int isc_info_svc_get_license = 51; /* Retrieves all license keys and IDs from the license file	*/
	internal const int isc_info_svc_get_license_mask = 52;    /* Retrieves a bitmask representing	licensed options on	the	server */
	internal const int isc_info_svc_get_config = 53;  /* Retrieves the parameters	and	values for IB_CONFIG */
	internal const int isc_info_svc_version = 54; /* Retrieves the version of	the	services manager */
	internal const int isc_info_svc_server_version = 55;  /* Retrieves the version of	the	InterBase server */
	internal const int isc_info_svc_implementation = 56;  /* Retrieves the implementation	of the InterBase server	*/
	internal const int isc_info_svc_capabilities = 57;    /* Retrieves a bitmask representing	the	server's capabilities */
	internal const int isc_info_svc_user_dbpath = 58; /* Retrieves the path to the security database in use by the server	*/
	internal const int isc_info_svc_get_env = 59; /* Retrieves the setting of	$INTERBASE */
	internal const int isc_info_svc_get_env_lock = 60;    /* Retrieves the setting of	$INTERBASE_LCK */
	internal const int isc_info_svc_get_env_msg = 61; /* Retrieves the setting of	$INTERBASE_MSG */
	internal const int isc_info_svc_line = 62;    /* Retrieves 1 line	of service output per call */
	internal const int isc_info_svc_to_eof = 63;  /* Retrieves as much of	the	server output as will fit in the supplied buffer */
	internal const int isc_info_svc_timeout = 64; /* Sets	/ signifies	a timeout value	for	reading	service	information	*/
	internal const int isc_info_svc_get_licensed_users = 65;  /* Retrieves the number	of users licensed for accessing	the	server */
	internal const int isc_info_svc_limbo_trans = 66; /* Retrieve	the	limbo transactions */
	internal const int isc_info_svc_running = 67; /* Checks to see if	a service is running on	an attachment */
	internal const int isc_info_svc_get_users = 68;   /* Returns the user	information	from isc_action_svc_display_users */
	internal const int isc_info_svc_stdin = 78;   /* Returns size of data, needed as stdin for service */

	#endregion

	#region Services Properties

	internal const int isc_spb_prp_page_buffers = 5;
	internal const int isc_spb_prp_sweep_interval = 6;
	internal const int isc_spb_prp_shutdown_db = 7;
	internal const int isc_spb_prp_deny_new_attachments = 9;
	internal const int isc_spb_prp_deny_new_transactions = 10;
	internal const int isc_spb_prp_reserve_space = 11;
	internal const int isc_spb_prp_write_mode = 12;
	internal const int isc_spb_prp_access_mode = 13;
	internal const int isc_spb_prp_set_sql_dialect = 14;

	internal const int isc_spb_prp_force_shutdown = 41;
	internal const int isc_spb_prp_attachments_shutdown = 42;
	internal const int isc_spb_prp_transactions_shutdown = 43;
	internal const int isc_spb_prp_shutdown_mode = 44;
	internal const int isc_spb_prp_online_mode = 45;

	internal const int isc_spb_prp_sm_normal = 0;
	internal const int isc_spb_prp_sm_multi = 1;
	internal const int isc_spb_prp_sm_single = 2;
	internal const int isc_spb_prp_sm_full = 3;

	// RESERVE_SPACE_PARAMETERS
	internal const int isc_spb_prp_res_use_full = 35;
	internal const int isc_spb_prp_res = 36;

	// WRITE_MODE_PARAMETERS
	internal const int isc_spb_prp_wm_async = 37;
	internal const int isc_spb_prp_wm_sync = 38;

	// ACCESS_MODE_PARAMETERS
	internal const int isc_spb_prp_am_readonly = 39;
	internal const int isc_spb_prp_am_readwrite = 40;

	// Option Flags
	internal const int isc_spb_prp_activate = 0x0100;
	internal const int isc_spb_prp_db_online = 0x0200;
	internal const int isc_spb_prp_nolinger = 0x0400;

	#endregion

	#region Backup Service

	internal const int isc_spb_bkp_file = 5;
	internal const int isc_spb_bkp_factor = 6;
	internal const int isc_spb_bkp_length = 7;
	internal const int isc_spb_bkp_skip_data = 8;
	internal const int isc_spb_bkp_stat = 15;
	internal const int isc_spb_bkp_keyholder = 16;
	internal const int isc_spb_bkp_keyname = 17;
	internal const int isc_spb_bkp_crypt = 18;
	internal const int isc_spb_bkp_include_data = 19;
	internal const int isc_spb_bkp_parallel_workers = 21;
	internal const int isc_spb_bkp_ignore_checksums = 0x01;
	internal const int isc_spb_bkp_ignore_limbo = 0x02;
	internal const int isc_spb_bkp_metadata_only = 0x04;
	internal const int isc_spb_bkp_no_garbage_collect = 0x08;
	internal const int isc_spb_bkp_old_descriptions = 0x10;
	internal const int isc_spb_bkp_non_transportable = 0x20;
	internal const int isc_spb_bkp_convert = 0x40;
	internal const int isc_spb_bkp_expand = 0x80;
	internal const int isc_spb_bkp_no_triggers = 0x8000;
	internal const int isc_spb_bkp_zip = 0x010000;
	internal const int isc_spb_bkp_direct_io = 0x020000;

	#endregion

	#region Restore Service

	internal const int isc_spb_res_skip_data = isc_spb_bkp_skip_data;
	internal const int isc_spb_res_include_data = isc_spb_bkp_include_data;
	internal const int isc_spb_res_buffers = 9;
	internal const int isc_spb_res_page_size = 10;
	internal const int isc_spb_res_length = 11;
	internal const int isc_spb_res_access_mode = 12;
	internal const int isc_spb_res_fix_fss_data = 13;
	internal const int isc_spb_res_fix_fss_metadata = 14;
	internal const int isc_spb_res_keyholder = isc_spb_bkp_keyholder;
	internal const int isc_spb_res_keyname = isc_spb_bkp_keyname;
	internal const int isc_spb_res_crypt = isc_spb_bkp_crypt;
	internal const int isc_spb_res_stat = isc_spb_bkp_stat;
	internal const int isc_spb_res_parallel_workers = isc_spb_bkp_parallel_workers;
	internal const int isc_spb_res_metadata_only = isc_spb_bkp_metadata_only;
	internal const int isc_spb_res_deactivate_idx = 0x0100;
	internal const int isc_spb_res_no_shadow = 0x0200;
	internal const int isc_spb_res_no_validity = 0x0400;
	internal const int isc_spb_res_one_at_a_time = 0x0800;
	internal const int isc_spb_res_replace = 0x1000;
	internal const int isc_spb_res_create = 0x2000;
	internal const int isc_spb_res_use_all_space = 0x4000;
	internal const int isc_spb_res_direct_io = isc_spb_bkp_direct_io;
	internal const int isc_spb_res_replica_mode = 20;

	internal const int isc_spb_res_am_readonly = isc_spb_prp_am_readonly;
	internal const int isc_spb_res_am_readwrite = isc_spb_prp_am_readwrite;

	#endregion

	#region Validate Service
	internal const int isc_spb_val_tab_incl = 1;  // include filter based on regular expression
	internal const int isc_spb_val_tab_excl = 2;  // exclude filter based on regular expression
	internal const int isc_spb_val_idx_incl = 3;  // regexp of indices to validate
	internal const int isc_spb_val_idx_excl = 4;  // regexp of indices to NOT validate
	internal const int isc_spb_val_lock_timeout = 5;  // how long to wait for table lock
	#endregion

	#region Repair Service

	internal const int isc_spb_rpr_commit_trans = 15;
	internal const int isc_spb_rpr_rollback_trans = 34;
	internal const int isc_spb_rpr_recover_two_phase = 17;
	internal const int isc_spb_tra_id = 18;
	internal const int isc_spb_single_tra_id = 19;
	internal const int isc_spb_multi_tra_id = 20;
	internal const int isc_spb_tra_state = 21;
	internal const int isc_spb_tra_state_limbo = 22;
	internal const int isc_spb_tra_state_commit = 23;
	internal const int isc_spb_tra_state_rollback = 24;
	internal const int isc_spb_tra_state_unknown = 25;
	internal const int isc_spb_tra_host_site = 26;
	internal const int isc_spb_tra_remote_site = 27;
	internal const int isc_spb_tra_db_path = 28;
	internal const int isc_spb_tra_advise = 29;
	internal const int isc_spb_tra_advise_commit = 30;
	internal const int isc_spb_tra_advise_rollback = 31;
	internal const int isc_spb_tra_advise_unknown = 33;
	internal const int isc_spb_tra_id_64 = 46;
	internal const int isc_spb_single_tra_id_64 = 47;
	internal const int isc_spb_multi_tra_id_64 = 48;
	internal const int isc_spb_rpr_commit_trans_64 = 49;
	internal const int isc_spb_rpr_rollback_trans_64 = 50;
	internal const int isc_spb_rpr_recover_two_phase_64 = 51;
	internal const int isc_spb_rpr_par_workers = 52;

	internal const int isc_spb_rpr_validate_db = 0x01;
	internal const int isc_spb_rpr_sweep_db = 0x02;
	internal const int isc_spb_rpr_mend_db = 0x04;
	internal const int isc_spb_rpr_list_limbo_trans = 0x08;
	internal const int isc_spb_rpr_check_db = 0x10;
	internal const int isc_spb_rpr_ignore_checksum = 0x20;
	internal const int isc_spb_rpr_kill_shadows = 0x40;
	internal const int isc_spb_rpr_full = 0x80;
	internal const int isc_spb_rpr_icu = 0x0800;

	#endregion

	#region Security Service

	internal const int isc_spb_sec_userid = 5;
	internal const int isc_spb_sec_groupid = 6;
	internal const int isc_spb_sec_username = 7;
	internal const int isc_spb_sec_password = 8;
	internal const int isc_spb_sec_groupname = 9;
	internal const int isc_spb_sec_firstname = 10;
	internal const int isc_spb_sec_middlename = 11;
	internal const int isc_spb_sec_lastname = 12;

	#endregion

	#region NBackup Service
	internal const int isc_spb_nbk_level = 5;
	internal const int isc_spb_nbk_file = 6;
	internal const int isc_spb_nbk_direct = 7;
	internal const int isc_spb_nbk_no_triggers = 0x01;
	#endregion

	#region Trace Service

	internal const int isc_spb_trc_id = 1;
	internal const int isc_spb_trc_name = 2;
	internal const int isc_spb_trc_cfg = 3;

	#endregion

	#region Configuration Keys

	internal const int ISCCFG_LOCKMEM_KEY = 0;
	internal const int ISCCFG_LOCKSEM_KEY = 1;
	internal const int ISCCFG_LOCKSIG_KEY = 2;
	internal const int ISCCFG_EVNTMEM_KEY = 3;
	internal const int ISCCFG_DBCACHE_KEY = 4;
	internal const int ISCCFG_PRIORITY_KEY = 5;
	internal const int ISCCFG_IPCMAP_KEY = 6;
	internal const int ISCCFG_MEMMIN_KEY = 7;
	internal const int ISCCFG_MEMMAX_KEY = 8;
	internal const int ISCCFG_LOCKORDER_KEY = 9;
	internal const int ISCCFG_ANYLOCKMEM_KEY = 10;
	internal const int ISCCFG_ANYLOCKSEM_KEY = 11;
	internal const int ISCCFG_ANYLOCKSIG_KEY = 12;
	internal const int ISCCFG_ANYEVNTMEM_KEY = 13;
	internal const int ISCCFG_LOCKHASH_KEY = 14;
	internal const int ISCCFG_DEADLOCK_KEY = 15;
	internal const int ISCCFG_LOCKSPIN_KEY = 16;
	internal const int ISCCFG_CONN_TIMEOUT_KEY = 17;
	internal const int ISCCFG_DUMMY_INTRVL_KEY = 18;
	internal const int ISCCFG_TRACE_POOLS_KEY = 19; /* Internal Use only	*/
	internal const int ISCCFG_REMOTE_BUFFER_KEY = 20;

	#endregion

	#region Common Structural Codes

	internal const int isc_info_end = 1;
	internal const int isc_info_truncated = 2;
	internal const int isc_info_error = 3;
	internal const int isc_info_data_not_ready = 4;
	internal const int isc_info_flag_end = 127;

	#endregion

	#region SQL Information

	internal const int isc_info_sql_select = 4;
	internal const int isc_info_sql_bind = 5;
	internal const int isc_info_sql_num_variables = 6;
	internal const int isc_info_sql_describe_vars = 7;
	internal const int isc_info_sql_describe_end = 8;
	internal const int isc_info_sql_sqlda_seq = 9;
	internal const int isc_info_sql_message_seq = 10;
	internal const int isc_info_sql_type = 11;
	internal const int isc_info_sql_sub_type = 12;
	internal const int isc_info_sql_scale = 13;
	internal const int isc_info_sql_length = 14;
	internal const int isc_info_sql_null_ind = 15;
	internal const int isc_info_sql_field = 16;
	internal const int isc_info_sql_relation = 17;
	internal const int isc_info_sql_owner = 18;
	internal const int isc_info_sql_alias = 19;
	internal const int isc_info_sql_sqlda_start = 20;
	internal const int isc_info_sql_stmt_type = 21;
	internal const int isc_info_sql_get_plan = 22;
	internal const int isc_info_sql_records = 23;
	internal const int isc_info_sql_batch_fetch = 24;
	internal const int isc_info_sql_relation_alias = 25;
	internal const int isc_info_sql_explain_plan = 26;
	internal const int isc_info_sql_stmt_flags = 27;

	#endregion

	#region SQL Information Return Values

	internal const int isc_info_sql_stmt_select = 1;
	internal const int isc_info_sql_stmt_insert = 2;
	internal const int isc_info_sql_stmt_update = 3;
	internal const int isc_info_sql_stmt_delete = 4;
	internal const int isc_info_sql_stmt_ddl = 5;
	internal const int isc_info_sql_stmt_get_segment = 6;
	internal const int isc_info_sql_stmt_put_segment = 7;
	internal const int isc_info_sql_stmt_exec_procedure = 8;
	internal const int isc_info_sql_stmt_start_trans = 9;
	internal const int isc_info_sql_stmt_commit = 10;
	internal const int isc_info_sql_stmt_rollback = 11;
	internal const int isc_info_sql_stmt_select_for_upd = 12;
	internal const int isc_info_sql_stmt_set_generator = 13;
	internal const int isc_info_sql_stmt_savepoint = 14;

	#endregion

	#region Database Information

	internal const int isc_info_db_id = 4;
	internal const int isc_info_reads = 5;
	internal const int isc_info_writes = 6;
	internal const int isc_info_fetches = 7;
	internal const int isc_info_marks = 8;

	internal const int isc_info_implementation = 11;
	internal const int isc_info_isc_version = 12;
	internal const int isc_info_base_level = 13;
	internal const int isc_info_page_size = 14;
	internal const int isc_info_num_buffers = 15;
	internal const int isc_info_limbo = 16;
	internal const int isc_info_current_memory = 17;
	internal const int isc_info_max_memory = 18;
	internal const int isc_info_window_turns = 19;
	internal const int isc_info_license = 20;

	internal const int isc_info_allocation = 21;
	internal const int isc_info_attachment_id = 22;
	internal const int isc_info_read_seq_count = 23;
	internal const int isc_info_read_idx_count = 24;
	internal const int isc_info_insert_count = 25;
	internal const int isc_info_update_count = 26;
	internal const int isc_info_delete_count = 27;
	internal const int isc_info_backout_count = 28;
	internal const int isc_info_purge_count = 29;
	internal const int isc_info_expunge_count = 30;

	internal const int isc_info_sweep_interval = 31;
	internal const int isc_info_ods_version = 32;
	internal const int isc_info_ods_minor_version = 33;
	internal const int isc_info_no_reserve = 34;
	internal const int isc_info_logfile = 35;
	internal const int isc_info_cur_logfile_name = 36;
	internal const int isc_info_cur_log_part_offset = 37;
	internal const int isc_info_num_wal_buffers = 38;
	internal const int isc_info_wal_buffer_size = 39;
	internal const int isc_info_wal_ckpt_length = 40;

	internal const int isc_info_wal_cur_ckpt_interval = 41;
	internal const int isc_info_wal_prv_ckpt_fname = 42;
	internal const int isc_info_wal_prv_ckpt_poffset = 43;
	internal const int isc_info_wal_recv_ckpt_fname = 44;
	internal const int isc_info_wal_recv_ckpt_poffset = 45;
	internal const int isc_info_wal_grpc_wait_usecs = 47;
	internal const int isc_info_wal_num_io = 48;
	internal const int isc_info_wal_avg_io_size = 49;
	internal const int isc_info_wal_num_commits = 50;

	internal const int isc_info_wal_avg_grpc_size = 51;
	internal const int isc_info_forced_writes = 52;
	internal const int isc_info_user_names = 53;
	internal const int isc_info_page_errors = 54;
	internal const int isc_info_record_errors = 55;
	internal const int isc_info_bpage_errors = 56;
	internal const int isc_info_dpage_errors = 57;
	internal const int isc_info_ipage_errors = 58;
	internal const int isc_info_ppage_errors = 59;
	internal const int isc_info_tpage_errors = 60;

	internal const int isc_info_set_page_buffers = 61;
	internal const int isc_info_db_sql_dialect = 62;
	internal const int isc_info_db_read_only = 63;
	internal const int isc_info_db_size_in_pages = 64;

	internal const int frb_info_att_charset = 101;
	internal const int isc_info_db_class = 102;
	internal const int isc_info_firebird_version = 103;
	internal const int isc_info_oldest_transaction = 104;
	internal const int isc_info_oldest_active = 105;
	internal const int isc_info_oldest_snapshot = 106;
	internal const int isc_info_next_transaction = 107;
	internal const int isc_info_db_provider = 108;
	internal const int isc_info_active_transactions = 109;

	internal const int isc_info_active_tran_count = 110;
	internal const int isc_info_creation_date = 111;
	internal const int isc_info_db_file_size = 112;
	internal const int fb_info_page_contents = 113;

	internal const int fb_info_implementation = 114;

	internal const int fb_info_page_warns = 115;
	internal const int fb_info_record_warns = 116;
	internal const int fb_info_bpage_warns = 117;
	internal const int fb_info_dpage_warns = 118;
	internal const int fb_info_ipage_warns = 119;
	internal const int fb_info_ppage_warns = 120;
	internal const int fb_info_tpage_warns = 121;
	internal const int fb_info_pip_errors = 122;
	internal const int fb_info_pip_warns = 123;

	internal const int fb_info_pages_used = 124;
	internal const int fb_info_pages_free = 125;

	internal const int fb_info_ses_idle_timeout_db = 129;
	internal const int fb_info_ses_idle_timeout_att = 130;
	internal const int fb_info_ses_idle_timeout_run = 131;

	internal const int fb_info_conn_flags = 132;

	internal const int fb_info_crypt_key = 133;
	internal const int fb_info_crypt_state = 134;

	internal const int fb_info_statement_timeout_db = 135;
	internal const int fb_info_statement_timeout_att = 136;

	internal const int fb_info_protocol_version = 137;
	internal const int fb_info_crypt_plugin = 138;

	internal const int fb_info_creation_timestamp_tz = 139;

	internal const int fb_info_wire_crypt = 140;

	internal const int fb_info_features = 141;

	internal const int fb_info_next_attachment = 142;
	internal const int fb_info_next_statement = 143;

	internal const int fb_info_db_guid = 144;
	internal const int fb_info_db_file_id = 145;

	internal const int fb_info_replica_mode = 146;

	internal const int fb_info_username = 147;
	internal const int fb_info_sqlrole = 148;

	#endregion

	#region Information Request

	internal const int isc_info_number_messages = 4;
	internal const int isc_info_max_message = 5;
	internal const int isc_info_max_send = 6;
	internal const int isc_info_max_receive = 7;
	internal const int isc_info_state = 8;
	internal const int isc_info_message_number = 9;
	internal const int isc_info_message_size = 10;
	internal const int isc_info_request_cost = 11;
	internal const int isc_info_access_path = 12;
	internal const int isc_info_req_select_count = 13;
	internal const int isc_info_req_insert_count = 14;
	internal const int isc_info_req_update_count = 15;
	internal const int isc_info_req_delete_count = 16;

	#endregion

	#region Array Slice Description Language

	internal const int isc_sdl_version1 = 1;
	internal const int isc_sdl_eoc = 255;
	internal const int isc_sdl_relation = 2;
	internal const int isc_sdl_rid = 3;
	internal const int isc_sdl_field = 4;
	internal const int isc_sdl_fid = 5;
	internal const int isc_sdl_struct = 6;
	internal const int isc_sdl_variable = 7;
	internal const int isc_sdl_scalar = 8;
	internal const int isc_sdl_tiny_integer = 9;
	internal const int isc_sdl_short_integer = 10;
	internal const int isc_sdl_long_integer = 11;
	internal const int isc_sdl_literal = 12;
	internal const int isc_sdl_add = 13;
	internal const int isc_sdl_subtract = 14;
	internal const int isc_sdl_multiply = 15;
	internal const int isc_sdl_divide = 16;
	internal const int isc_sdl_negate = 17;
	internal const int isc_sdl_eql = 18;
	internal const int isc_sdl_neq = 19;
	internal const int isc_sdl_gtr = 20;
	internal const int isc_sdl_geq = 21;
	internal const int isc_sdl_lss = 22;
	internal const int isc_sdl_leq = 23;
	internal const int isc_sdl_and = 24;
	internal const int isc_sdl_or = 25;
	internal const int isc_sdl_not = 26;
	internal const int isc_sdl_while = 27;
	internal const int isc_sdl_assignment = 28;
	internal const int isc_sdl_label = 29;
	internal const int isc_sdl_leave = 30;
	internal const int isc_sdl_begin = 31;
	internal const int isc_sdl_end = 32;
	internal const int isc_sdl_do3 = 33;
	internal const int isc_sdl_do2 = 34;
	internal const int isc_sdl_do1 = 35;
	internal const int isc_sdl_element = 36;

	#endregion

	#region Blob Parameter Block

	internal const int isc_bpb_version1 = 1;
	internal const int isc_bpb_source_type = 1;
	internal const int isc_bpb_target_type = 2;
	internal const int isc_bpb_type = 3;
	internal const int isc_bpb_source_interp = 4;
	internal const int isc_bpb_target_interp = 5;
	internal const int isc_bpb_filter_parameter = 6;

	internal const int isc_bpb_type_segmented = 0;
	internal const int isc_bpb_type_stream = 1;

	internal const int RBL_eof = 1;
	internal const int RBL_segment = 2;
	internal const int RBL_eof_pending = 4;
	internal const int RBL_create = 8;

	#endregion

	#region Blob Information

	internal const int isc_info_blob_num_segments = 4;
	internal const int isc_info_blob_max_segment = 5;
	internal const int isc_info_blob_total_length = 6;
	internal const int isc_info_blob_type = 7;

	#endregion

	#region Event Codes

	internal const int P_REQ_async = 1;   // Auxilliary asynchronous port
	internal const int EPB_version1 = 1;

	#endregion

	#region ISC Error codes

	internal const int isc_facility = 20;
	internal const int isc_err_base = 335544320;
	internal const int isc_err_factor = 1;
	internal const int isc_arg_end = 0;    // end of argument list
	internal const int isc_arg_gds = 1;    // generic DSRI	status value
	internal const int isc_arg_string = 2;    // string argument
	internal const int isc_arg_cstring = 3;   // count & string argument
	internal const int isc_arg_number = 4;    // numeric argument	(long)
	internal const int isc_arg_interpreted = 5;   // interpreted status code (string)
	internal const int isc_arg_vms = 6;   // VAX/VMS status code (long)
	internal const int isc_arg_unix = 7;  // UNIX	error code
	internal const int isc_arg_domain = 8;    // Apollo/Domain error code
	internal const int isc_arg_dos = 9;   // MSDOS/OS2 error code
	internal const int isc_arg_mpexl = 10;    // HP MPE/XL error code
	internal const int isc_arg_mpexl_ipc = 11;    // HP MPE/XL IPC error code
	internal const int isc_arg_next_mach = 15;    // NeXT/Mach error code
	internal const int isc_arg_netware = 16;  // NetWare error code
	internal const int isc_arg_win32 = 17;    // Win32 error code
	internal const int isc_arg_warning = 18;  // warning argument
	internal const int isc_arg_sql_state = 19;    // SQLSTATE

	internal const int isc_open_trans = 335544357;
	internal const int isc_segment = 335544366;
	internal const int isc_segstr_eof = 335544367;
	internal const int isc_connect_reject = 335544421;
	internal const int isc_invalid_dimension = 335544458;
	internal const int isc_tra_state = 335544468;
	internal const int isc_except = 335544517;
	internal const int isc_dsql_sqlda_err = 335544583;
	internal const int isc_network_error = 335544721;
	internal const int isc_net_read_err = 335544726;
	internal const int isc_net_write_err = 335544727;
	internal const int isc_stack_trace = 335544842;
	internal const int isc_except2 = 335544848;
	internal const int isc_arith_except = 335544321;
	internal const int isc_string_truncation = 335544914;
	internal const int isc_formatted_exception = 335545016;
	internal const int isc_wirecrypt_incompatible = 335545064;
	internal const int isc_cancelled = 335544794;
	internal const int isc_nothing_to_cancel = 335544933;

	#endregion

	#region BLR Codes

	internal const int blr_version5 = 5;
	internal const int blr_begin = 2;
	internal const int blr_message = 4;
	internal const int blr_eoc = 76;
	internal const int blr_end = 255;

	internal const int blr_text = 14;
	internal const int blr_text2 = 15;
	internal const int blr_short = 7;
	internal const int blr_long = 8;
	internal const int blr_quad = 9;
	internal const int blr_int64 = 16;
	internal const int blr_float = 10;
	internal const int blr_double = 27;
	internal const int blr_d_float = 11;
	internal const int blr_timestamp = 35;
	internal const int blr_varying = 37;
	internal const int blr_varying2 = 38;
	internal const int blr_blob = 261;
	internal const int blr_cstring = 40;
	internal const int blr_cstring2 = 41;
	internal const int blr_blob_id = 45;
	internal const int blr_sql_date = 12;
	internal const int blr_sql_time = 13;
	internal const int blr_bool = 23;
	internal const int blr_dec64 = 24;
	internal const int blr_dec128 = 25;
	internal const int blr_int128 = 26;
	internal const int blr_sql_time_tz = 28;
	internal const int blr_timestamp_tz = 29;
	internal const int blr_ex_time_tz = 30;
	internal const int blr_ex_timestamp_tz = 31;

	internal const int blr_null = 45;

	#endregion

	#region DataType Definitions

	internal const int SQL_TEXT = 452;
	internal const int SQL_VARYING = 448;
	internal const int SQL_SHORT = 500;
	internal const int SQL_LONG = 496;
	internal const int SQL_FLOAT = 482;
	internal const int SQL_DOUBLE = 480;
	internal const int SQL_D_FLOAT = 530;
	internal const int SQL_TIMESTAMP = 510;
	internal const int SQL_BLOB = 520;
	internal const int SQL_ARRAY = 540;
	internal const int SQL_QUAD = 550;
	internal const int SQL_TYPE_TIME = 560;
	internal const int SQL_TYPE_DATE = 570;
	internal const int SQL_INT64 = 580;
	internal const int SQL_TIMESTAMP_TZ_EX = 32748;
	internal const int SQL_TIME_TZ_EX = 32750;
	internal const int SQL_INT128 = 32752;
	internal const int SQL_TIMESTAMP_TZ = 32754;
	internal const int SQL_TIME_TZ = 32756;
	internal const int SQL_DEC16 = 32760;
	internal const int SQL_DEC34 = 32762;
	internal const int SQL_BOOLEAN = 32764;
	internal const int SQL_NULL = 32766;

	// Historical alias	for	pre	V6 applications
	internal const int SQL_DATE = SQL_TIMESTAMP;

	#endregion

	#region Cancel types

	internal const int fb_cancel_disable = 1;
	internal const int fb_cancel_enable = 2;
	internal const int fb_cancel_raise = 3;
	internal const int fb_cancel_abort = 4;

	#endregion

	#region User identification data

	internal const int CNCT_user = 1;
	internal const int CNCT_passwd = 2;
	internal const int CNCT_host = 4;
	internal const int CNCT_group = 5;
	internal const int CNCT_user_verification = 6;
	internal const int CNCT_specific_data = 7;
	internal const int CNCT_plugin_name = 8;
	internal const int CNCT_login = 9;
	internal const int CNCT_plugin_list = 10;
	internal const int CNCT_client_crypt = 11;

	#endregion

	#region Transaction information items

	internal const int isc_info_tra_id = 4;
	internal const int isc_info_tra_oldest_interesting = 5;
	internal const int isc_info_tra_oldest_snapshot = 6;
	internal const int isc_info_tra_oldest_active = 7;
	internal const int isc_info_tra_isolation = 8;
	internal const int isc_info_tra_access = 9;
	internal const int isc_info_tra_lock_timeout = 10;
	internal const int fb_info_tra_dbpath = 11;
	internal const int fb_info_tra_snapshot_number = 12;

	// isc_info_tra_isolation responses
	internal const int isc_info_tra_consistency = 1;
	internal const int isc_info_tra_concurrency = 2;
	internal const int isc_info_tra_read_committed = 3;

	// isc_info_tra_read_committed options
	internal const int isc_info_tra_no_rec_version = 0;
	internal const int isc_info_tra_rec_version = 1;
	internal const int isc_info_tra_read_consistency = 2;

	// isc_info_tra_access responses
	internal const int isc_info_tra_readonly = 0;
	internal const int isc_info_tra_readwrite = 1;

	#endregion

	internal static class Batch
	{
		internal const int VERSION1 = 1;

		internal const int TAG_MULTIERROR = 1;
		internal const int TAG_RECORD_COUNTS = 2;
		internal const int TAG_BUFFER_BYTES_SIZE = 3;
		internal const int TAG_BLOB_POLICY = 4;
		internal const int TAG_DETAILED_ERRORS = 5;

		internal const int BLOB_NONE = 0;
		internal const int BLOB_ID_ENGINE = 1;
		internal const int BLOB_ID_USER = 2;
		internal const int BLOB_STREAM = 3;

		internal const int BLOB_SEGHDR_ALIGN = 2;
	}
}
