Pogo
====
Pogo is an unofficial ORM for [Google's Cloud Datastore](https://developers.google.com/datastore/) (GCD).  The API was inspired by the [RavenDB](http://www.ravendb.net) [client API](http://ravendb.net/docs/2.0/client-api).

Installation
------------
Pogo is available on [Nuget](https://nuget.org/packages/pogo/).

Setting up a GCD Dataset
------------------------
You can follow Google's instructions [here](https://developers.google.com/datastore/docs/activate#google_cloud_datastore_from_other_platforms)

Connection String
-----------------
Pogo uses the standard config file connection strings:

    <configuration>
        <connectionStrings>
            <add name="Pogo" connectionString="DatasetId = YOUR_DATASET_ID; CertificateFilePath = C:\Path\To\Your\Certificate\File-privatekey.p12; ServiceAccountId = your-service-account-email@developer.gserviceaccount.com; CertificatePassword = notasecret"/>
        </connectionStrings>
    </configuration>

__DatasetId__: GCD Dataset ID (same as Google Cloud Project ID)

__CertificateFilePath__: Path to the private key file generated from step 10 [here](https://developers.google.com/datastore/docs/activate#google_cloud_datastore_from_other_platforms)

__ServiceAccountId__: The service account from step 11 [here](https://developers.google.com/datastore/docs/activate#google_cloud_datastore_from_other_platforms)

__CertificatePassword__: The password to the certificate (usually "notasecret")
    

Initialization
--------------
    var datastore = new GoogleCloudDatastore("ConnStringName");

Writing Objects
---------------
    var person = new TestPerson
    {
        Id = "TestPersons/99",
        FirstName = "Rey",
        HireDate = new DateTime(2012, 5, 7, 9, 30, 0),
        HourlyRate = 25.0,
        IsActive = true
    };

    using (var session = datastore.OpenSession())
    {
        session.Store(person);
        session.SaveChanges();
    }

Retrieving Objects by ID
------------------------
    using (var session = datastore.OpenSession())
    {
        var person = session.Load<TestPerson>("TestPersons/99");
    }

Sessions
--------
All interactions with GCD happen within the context of a session.  Pogo implements the Unit of Work pattern.  If an object is retrieved within a session, modified, and SaveChanges is called, then those changes will be applied to the datastore.

Querying
--------
GCD supports comparison operators (equal, less than, greater than, less than or equal, greater than or equal).  Pogo supports LINQ and translates these queries into low-level GCD queries.

    using (var session = datastore.OpenSession())
    {
        var results = session.Query<TestPerson>()
            .Where(t => t.HourlyRate > 50.0)
            .ToList();
    }

Indexes
-------
All GCD queries use indexes.  For basic queries, indexes are auto-generated.  If inequality filters are applied to multiple properties, you need to explicitly create an index.

If GCD doesn't support auto-indexing for complex queries soon, then we will add index generation to Pogo.  For now, you need to [create manual indexes](https://developers.google.com/datastore/docs/tools/indexconfig#Manual_Index_Configuration) and [publish them to GCD](https://developers.google.com/datastore/docs/tools/indexconfig#Updating_Indexes) using the [gcd tool](https://developers.google.com/datastore/docs/downloads#tools).

Contributing
------------
See the CONTRIBUTING document.

About
-----
Pogo is written and maintained by Samuel Nissim.

License
-------
Pogo is free software, and may be redistributed under the terms specified in the LICENSE file.
