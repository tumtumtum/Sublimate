# Sublimate CodeGen Tool

A tool for generating cross-platform type-safe and asnchronous APIs for HTTP based services.

** Sublimate is a work in progress and is not complete **

## The problem

Mobile and desktop apps today connect to backend XML or JSON based REST/HTTP Web Services. These apps and backend webservices are often run on a variety of platforms and programming languages.

If you're an app developer, you should not have to care that these web service calls are made over the internet rather than locally in-process. You can abstract these calls by hiding all the networking code behind manually coded gateways (wrappers around your backend web services).  The problem with doing this is:

 * As an app developer you should not be expected to know low level networking
 * Manually coded  gateways need to be manually updated every time a service is added or changed
 * Manually coded gateways usually need to use slow dynamic runtime serialisers 
 * Manually coded gateways are prone to manual coding errors or mistakes
 * If you have multiple apps on multiple platforms (iOS, Android, HTML+JS, C++) you have to manually maintain the gateways for all those platforms
 * They break slow down your development cycle as new backend services can't be tested immediately


## Sublimate solution

Sublimate will will automatically generate C#, QT/C++, Objective-C, Java, Android and HTML/Javascript APIs for XML and JSON based HTTP based web services. Don't worry about needing to use WebRequest, QSocket, NSURLConnection, HttpClient or XMLHTTPRequest for RPC calls ever again.

If you have a web service accessible at the URL: http://www.example.com/SearchService/Search?query=Hello that returns a strongly typed object, you can will be able to use the following code:

	
C#
	
	searchService.Search("Hello", searchResult => Console.WriteLine(searchResult.TotalCount));


C++
	
	searchService.Search("Hello", this);
	
	-(void) RequestListener<SearchResult>::Response(SearchResult* response)
	{
		std::out << searchResult->getTotalCount();
	}
	
	
Objective-C
	
	SearchResult* result = searchService.search(@"Hello", ^(SearchResult* response)
	{
		NSLog("%d", response.totalCount);
	});

Java

	searchService.search("Hello", new RequestListener<SearchResult>()
	{
		void onComplete(SearchResult searchResult)
		{
			System.out.println(searchResult.getTotalCount());
		}
	});
	
Javascript

	searchService.search("Hello", function(searchResult)
	{
		alert(searchResult.totalCount);
	});
	
	
## Copyright

Copyright (2013) Thong Nguyen (tumtumtum@gmail.com)
	