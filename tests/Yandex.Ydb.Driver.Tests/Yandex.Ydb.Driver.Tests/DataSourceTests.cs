namespace Yandex.Ydb.Driver.Tests;

public class DataSourceTests
{
    [Fact]
    public void DataSourceWithSsl_NullCertPath_IsThrowException()
    {
        var exception = Assert.Throws<YdbDriverException>(() =>
        {
            using var dataSource =
                YdbDataSource.Create("Host=localhost;Port=2135;UseSsl=true;");
            var dbCommand = dataSource.CreateCommand("SELECT 1, Bool('true');");
        });

        Assert.IsType<InvalidDataException>(exception.InnerException);
    }

    [Fact]
    public void DataSourceWithSsl_WrongCertPath_IsThrowException()
    {
        var exception = Assert.Throws<YdbDriverException>(() =>
        {
            using var dataSource =
                YdbDataSource.Create("Host=localhost;Port=2135;UseSsl=true;RootCertificate=.\\wrong_certs;Pooling=true");
            var dbCommand = dataSource.CreateCommand("SELECT 1, Bool('true');");
        });

        Assert.IsType<FileNotFoundException>(exception.InnerException);
    }

    [Fact]
    public void DataSourceWithSsl_CustomCert_AndWithoutTrust_IsThrowException()
    {
        var exception = Assert.Throws<YdbDriverException>(() =>
        {
            using var dataSource =
                YdbDataSource.Create("Host=localhost;Port=2135;UseSsl=true;RootCertificate=.\\certs;Pooling=true");
            var dbCommand = dataSource.CreateCommand("SELECT 1, Bool('true');");
        });
    }

    [Fact]
    public void DataSourceWithSsl_IsWorking()
    {
        using var dataSource =
            YdbDataSource.Create("Host=localhost;Port=2135;UseSsl=true;RootCertificate=.\\certs;TrustSsl=true;Pooling=true");
        var dbCommand = dataSource.CreateCommand("SELECT 1, Bool('true');");
        var reader = dbCommand.ExecuteReader();
        var read = reader.Read();
        Assert.True(read);
        var int32 = reader.GetInt32(0);
        Assert.Equal(1, int32);

        var b = reader.GetBoolean(1);
        Assert.True(b);
    }

    [Fact]
    public void UnpooledYdbDataSource_CreateCommand_Success()
    {
        using var dataSource = YdbDataSource.Create("Host=localhost;Port=2136;Pooling=false");

        var dbCommand = dataSource.CreateCommand("SELECT 1, Bool('true');");
        var reader = dbCommand.ExecuteReader();
        var read = reader.Read();
        Assert.True(read);
        var int32 = reader.GetInt32(0);
        Assert.Equal(1, int32);

        var b = reader.GetBoolean(1);
        Assert.True(b);
    }

    [Fact]
    public void PooledYdbDataSource_CreateCommand_Success()
    {
        using var dataSource = YdbDataSource.Create("Host=localhost;Port=2136;Pooling=true");

        var dbCommand = dataSource.CreateCommand("SELECT 1, Bool('true');");
        var reader = dbCommand.ExecuteReader();
        var read = reader.Read();
        Assert.True(read);
        var int32 = reader.GetInt32(0);
        Assert.Equal(1, int32);

        var b = reader.GetBoolean(1);
        Assert.True(b);
    }

    [Fact]
    public async Task PooledYdbDataSource_IsPolledDataSource_True()
    {
        await using var dataSource = YdbDataSource.Create("Host=localhost;Port=2136;Pooling=true");
        var connection = await dataSource.OpenConnectionAsync();
        Assert.Equal((1, 0, 0), dataSource.Statistics);
        await connection.CloseAsync();
        Assert.Equal((1, 1, 1), dataSource.Statistics);
    }

    [Fact]
    public async Task PooledYdbDataSource_OpenThreeConnection_SuccessStatistics()
    {
        await using var dataSource = YdbDataSource.Create("Host=localhost;Port=2136;Pooling=true");
        var connection1 = await dataSource.OpenConnectionAsync();
        Assert.Equal((1, 0, 0), dataSource.Statistics);
        var connection2 = await dataSource.OpenConnectionAsync();
        Assert.Equal((2, 0, 0), dataSource.Statistics);
        var connection3 = await dataSource.OpenConnectionAsync();
        Assert.Equal((3, 0, 0), dataSource.Statistics);
        connection1.Close();
        Assert.Equal((3, 1, 1), dataSource.Statistics);
        connection2.Close();
        Assert.Equal((3, 2, 2), dataSource.Statistics);
        connection3.Close();
        Assert.Equal((3, 3, 3), dataSource.Statistics);
        var connection4 = await dataSource.OpenConnectionAsync();
        Assert.Equal((3, 2, 2), dataSource.Statistics);
    }
}