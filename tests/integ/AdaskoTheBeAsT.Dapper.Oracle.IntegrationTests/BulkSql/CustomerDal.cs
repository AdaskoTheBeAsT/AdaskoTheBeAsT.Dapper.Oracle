using System;
using System.Collections.Generic;
using System.Data;
using AdaskoTheBeAsT.Dapper.Oracle.BulkSql;

namespace AdaskoTheBeAsT.Dapper.Oracle.IntegrationTests.BulkSql;

public class CustomerDal
{
    public CustomerDal(IDbConnection connection)
    {
        ArgumentNullException.ThrowIfNull(connection);
        Connection = connection;
    }

    public IDbConnection Connection { get; }

    public int InsertCustomers(IEnumerable<Customer> customers)
    {
        ArgumentNullException.ThrowIfNull(customers);

        const string insertSql = """
                                 INSERT INTO BULKCUSTOMERS(CUSTOMERID,COMPANYNAME,ADDRESS,POSTALCODE,CITY, CONTACTNAME, CONTACTTITLE, Country, tidsstempel, opprettettid)
                                 VALUES(:CUSTOMERID,:COMPANYNAME,:ADDRESS, :POSTALCODE,:CITY, :ContactName, :CONTACTTITLE, :Country, :tidsstempel, :opprettettid)
                                 """;

        var mapping = new BulkMapping<Customer>[]
        {
            new("CUSTOMERID", static c => c.CustomerId),
            new("COMPANYNAME", static c => c.ContactName),
            new("ADDRESS", static c => c.Address),
            new("POSTALCODE", static c => c.PostalCode),
            new("CITY", static c => c.City),
            new("ContactName", static c => c.ContactName),
            new("ContactTitle", static c => c.ContactTitle),
            new("Country", static c => c.Country),
            new("Tidsstempel", static c => c.TidsStempel),
            new("Opprettettid", static c => c.OpprettetTid),
        };

        return Connection.SqlBulk(insertSql, customers, mapping);
    }
}
