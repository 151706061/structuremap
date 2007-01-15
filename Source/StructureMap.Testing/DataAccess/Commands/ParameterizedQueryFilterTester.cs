using System;
using System.Data;
using System.Data.SqlClient;
using NUnit.Framework;
using StructureMap.DataAccess.Commands;
using StructureMap.DataAccess.MSSQL;

namespace StructureMap.Testing.DataAccess.Commands
{
	[TestFixture]
	public class ParameterizedQueryFilterTester
	{
		private ParameterizedQueryFilter _filter;

		[SetUp]
		public void SetUp()
		{
			_filter = new ParameterizedQueryFilter("Name", "Column = {Value}");
		}

		[Test]
		public void InitializeAndGetWhereClause()
		{
			MSSQLDatabaseEngine engine = ObjectMother.MSSQLDatabaseEngine();
			IDbCommand command = engine.GetCommand();

			_filter.Initialize(engine, command);

			Assert.AreEqual(0, command.Parameters.Count);

			Assert.AreEqual("Column = @Name", _filter.GetWhereClause());
		}

		[Test]
		public void InitializeAndAttachParameters()
		{
			MSSQLDatabaseEngine engine = ObjectMother.MSSQLDatabaseEngine();
			IDbCommand command = engine.GetCommand();

			_filter.Initialize(engine, command);
			string theValue = "something";
			_filter.SetProperty(theValue);
			_filter.AttachParameters(command);

			Assert.AreEqual(1, command.Parameters.Count);
			SqlParameter parameter = (SqlParameter) command.Parameters[0];
		
			Assert.AreEqual("@Name", parameter.ParameterName);
			Assert.AreEqual(theValue, parameter.Value);
		}
	}
}
