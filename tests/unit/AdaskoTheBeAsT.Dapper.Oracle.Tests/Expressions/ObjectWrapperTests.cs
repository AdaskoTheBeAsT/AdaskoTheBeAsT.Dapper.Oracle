using System;
using System.Data;
using System.Linq;
using AdaskoTheBeAsT.Dapper.Oracle.Expressions;
using AwesomeAssertions;
using Xunit;

namespace AdaskoTheBeAsT.Dapper.Oracle.Tests.Expressions;

public class ObjectWrapperTests : ParameterBaseTests
{
    private interface ITestClass
    {
    }

    [Theory]
    [MemberData(nameof(OracleDataFixture), DisableDiscoveryEnumeration = true)]
    public void Test(IDbCommand cmd)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var param = cmd.CreateParameter();
        var wrapper = new ObjectWrapper<IDbDataParameter, int>("Size", param.GetType());
        wrapper.SetValue(param, 100);
        var result = wrapper.GetValue(param);
        result.Should().Be(100);
    }

    [Fact]
    public void SetPropertyOnClass()
    {
        var expected = Enumerable.Range(0, 10).ToArray();
        var input = new TestClass
        {
            PropertyWithGetterAndSetter = expected,
        };
        var testClass = new ObjectWrapper<TestClass, int[]>("PropertyWithGetterAndSetter", typeof(TestClass));

        var actual = testClass.GetValue(input);
        actual.Should().BeSameAs(expected);
    }

    [Fact]
    public void SetPropertyOnInterface()
    {
        var expected = Enumerable.Range(0, 10).ToArray();
        var input = new TestClass
        {
            PropertyWithGetterAndSetter = expected,
        };
        var testClass = new ObjectWrapper<ITestClass, int[]>("PropertyWithGetterAndSetter", typeof(TestClass));

        var actual = testClass.GetValue(input);
        actual.Should().BeSameAs(expected);
    }

    private sealed class TestClass : ITestClass
    {
        public int[] PropertyWithGetterAndSetter { get; set; } = [];
    }
}
