using System;



namespace BlackbirdSql.Core.Ctl;

public class SqlStyleUriParser(GenericUriParserOptions options) : GenericUriParser(options)
{

	protected override string GetComponents(Uri uri, UriComponents components, UriFormat format)
	{
		string originalString = uri.OriginalString;
		string passiveProtocol = "passive" + NativeDb.Protocol;

		string uriString = ReplaceProtocols(originalString, NativeDb.Protocol, passiveProtocol);

		Uri passiveUri = new(uriString);
		uriString = base.GetComponents(passiveUri, components, format);

		uriString = ReplaceProtocols(uriString, passiveProtocol, NativeDb.Protocol);

		// Tracer.Trace(GetType(), "GetComponents()", "\nOriginal: {0}\nResult: {1}", originalString, uriString);

		return uriString;
	}

	private string ReplaceProtocols(string originalString, string originalProtocol, string newProtocol)
	{
		string uriString;

		int pos = originalString.IndexOf(originalProtocol);
		int len = originalProtocol.Length;


		if (pos > 0)
			uriString = originalString[..pos] + newProtocol + originalString[(pos + len)..];
		else if (pos == 0)
			uriString = newProtocol + originalString[len..];
		else
			uriString = originalString;

		return uriString;
	}



	protected override void InitializeAndValidate(Uri uri, out UriFormatException parsingError)
	{
		parsingError = null;
	}



	protected override bool IsBaseOf(Uri baseUri, Uri relativeUri)
	{
		string passiveProtocol = "passive" + NativeDb.Protocol;
		string uriString = ReplaceProtocols(baseUri.OriginalString, NativeDb.Protocol, passiveProtocol);

		Uri passiveBaseUri = new(uriString);

		uriString = ReplaceProtocols(relativeUri.OriginalString, NativeDb.Protocol, passiveProtocol);

		Uri passiveRelativeUri = new(uriString);

		return base.IsBaseOf(passiveBaseUri, passiveRelativeUri);
	}



	protected override bool IsWellFormedOriginalString(Uri uri)
	{
		string passiveProtocol = "passive" + NativeDb.Protocol;
		string uriString = ReplaceProtocols(uri.OriginalString, NativeDb.Protocol, passiveProtocol);

		Uri passiveUri = new(uriString);

		return base.IsWellFormedOriginalString(passiveUri);
	}



	protected override void OnRegister(string schemeName, int defaultPort)
	{
		base.OnRegister(schemeName, defaultPort);
	}



	protected override string Resolve(Uri baseUri, Uri relativeUri, out UriFormatException parsingError)
	{
		string passiveProtocol = "passive" + NativeDb.Protocol;
		string uriString = ReplaceProtocols(baseUri.OriginalString, NativeDb.Protocol, passiveProtocol);

		Uri passiveBaseUri = new(uriString);


		uriString = ReplaceProtocols(relativeUri.OriginalString, NativeDb.Protocol, passiveProtocol);
		Uri passiveRelativeUri = new(uriString);

		uriString = base.Resolve(passiveBaseUri, passiveRelativeUri, out parsingError);
		uriString = ReplaceProtocols(uriString, passiveProtocol, NativeDb.Protocol);

		return uriString;
	}

}
