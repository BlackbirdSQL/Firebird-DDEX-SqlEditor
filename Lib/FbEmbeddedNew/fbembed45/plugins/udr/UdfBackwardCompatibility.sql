-- Create functions in current DB
create function div (
    n1 integer,
    n2 integer
) returns double precision
    external name 'udf_compat!UC_div'
    engine udr;

create function frac (
    val double precision
) returns double precision
    external name 'udf_compat!UC_frac'
    engine udr;

create function dow (
    val timestamp
) returns varchar(53) character set none
    external name 'udf_compat!UC_dow'
    engine udr;

create function sdow (
    val timestamp
) returns varchar(13) character set none
    external name 'udf_compat!UC_sdow'
    engine udr;

create function getExactTimestampUTC
	returns timestamp
    external name 'udf_compat!UC_getExactTimestampUTC'
    engine udr;

create function isLeapYear (
    val timestamp
) returns boolean
    external name 'udf_compat!UC_isLeapYear'
    engine udr;

-- Run minimum test
select 25, 3, div(25, 3) from rdb$database;
select pi(), frac(pi()) from rdb$database;
select timestamp '2020-01-29', dow(timestamp '2020-01-29'), sdow(timestamp '2020-01-29') from rdb$database;
set time zone 'utc';
select cast(((current_timestamp - getexacttimestamputc()) * 1000) as integer) as getexacttimestamptest from rdb$database;
set time zone local;
select timestamp '2019-01-29', isleapyear(timestamp '2019-01-29') from rdb$database;
select timestamp '2020-01-29', isleapyear(timestamp '2020-01-29') from rdb$database;
