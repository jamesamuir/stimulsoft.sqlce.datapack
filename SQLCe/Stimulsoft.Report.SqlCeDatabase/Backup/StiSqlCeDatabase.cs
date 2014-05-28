#region Copyright (C) 2003-2010 Stimulsoft
/*
{*******************************************************************}
{																	}
{	Stimulsoft Reports       										}
{																	}
{	Copyright (C) 2003-2010 Stimulsoft     							}
{	ALL RIGHTS RESERVED												}
{																	}
{	The entire contents of this file is protected by U.S. and		}
{	International Copyright Laws. Unauthorized reproduction,		}
{	reverse-engineering, and distribution of all or any portion of	}
{	the code contained in this file is strictly prohibited and may	}
{	result in severe civil and criminal penalties and will be		}
{	prosecuted to the maximum extent possible under the law.		}
{																	}
{	RESTRICTIONS													}
{																	}
{	THIS SOURCE CODE AND ALL RESULTING INTERMEDIATE FILES			}
{	ARE CONFIDENTIAL AND PROPRIETARY								}
{	TRADE SECRETS OF Stimulsoft										}
{																	}
{	CONSULT THE END USER LICENSE AGREEMENT FOR INFORMATION ON		}
{	ADDITIONAL RESTRICTIONS.										}
{																	}
{*******************************************************************}
*/
#endregion Copyright (C) 2003-2010 Stimulsoft
using System;
using System.Windows.Forms;
using System.ComponentModel;
using Stimulsoft.Base.Localization;
using Stimulsoft.Base.Serializing;
using Stimulsoft.Report.Dictionary.Design;
using System.Data.SqlServerCe;
using System.Collections;
using System.Data;

namespace Stimulsoft.Report.Dictionary
{
    [TypeConverter(typeof(StiSqlDatabaseConverter))]
    public class StiSqlCeDatabase : StiSqlDatabase
    {
        #region StiService override
        public override string ServiceName
        {
            get
            {
                return StiLocalization.Get("Database", "DatabaseSqlCe");
            }
        }
        #endregion

        #region DataAdapter override
        protected override string DataAdapterType
        {
            get
            {
                return "Stimulsoft.Report.Dictionary.StiSqlCeAdapterService";
            }
        }
        #endregion

        /// <summary>
        /// Returns full database information.
        /// </summary>
        public override StiDatabaseInformation GetDatabaseInformation()
        {
            StiDatabaseInformation information = new StiDatabaseInformation();
            try
            {
                using (SqlCeConnection connection = new SqlCeConnection(this.ConnectionString))
                {
                    connection.Open();

                    #region Tables
                    Hashtable tableHash = new Hashtable();
                    DataTable tables = connection.GetSchema("Tables");

                    foreach (DataRow row in tables.Rows)
                    {
                        DataTable table = new DataTable(row["TABLE_NAME"] as string);

                        tableHash[table.TableName] = table;
                        information.Tables.Add(table);
                    }

                    #endregion

                    #region Columns
                    DataTable columns = connection.GetSchema("Columns");

                    foreach (DataRow row in columns.Rows)
                    {
                        string columnName = row["COLUMN_NAME"] as string;
                        string tableName = row["TABLE_NAME"] as string;

                        Type columnType = ConvertDbTypeToTypeInternal(row["DATA_TYPE"] as string);

                        DataColumn column = new DataColumn(columnName, columnType);
                        DataTable table = tableHash[tableName] as DataTable;

                        if (table != null)
                        {
                            table.Columns.Add(column);
                        }
                    }
                    #endregion

                    connection.Close();
                }


                return information;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Adds tables, views and stored procedures to report dictionary from database information.
        /// </summary>
        public override void ApplyDatabaseInformation(StiDatabaseInformation information, StiReport report, StiDatabaseInformation informationAll)
        {
            #region Tables

            foreach (DataTable dataTable in information.Tables)
            {
                StiSqlCeSource source = new StiSqlCeSource(this.Name,
                    StiNameCreation.CreateName(report, dataTable.TableName, false, false, true));
                string table = dataTable.TableName;
                if (table.Trim().Contains(" ")) table = string.Format("[{0}]", table);
                source.SqlCommand = "select * from " + table;

                foreach (DataColumn dataColumn in dataTable.Columns)
                {
                    StiDataColumn column = new StiDataColumn(dataColumn.ColumnName, dataColumn.DataType);
                    source.Columns.Add(column);
                }
                report.Dictionary.DataSources.Add(source);
            }
            #endregion
        }

        public override void ApplyDatabaseInformation(StiDatabaseInformation information, StiReport report)
        {
            ApplyDatabaseInformation(information, report, null);
        }

        public override DialogResult Edit(bool newDatabase)
        {
            using (StiSqlDatabaseEditForm form = new StiSqlDatabaseEditForm(this))
            {
                if (newDatabase) form.Text = StiLocalization.Get("FormDatabaseEdit", "SqlCeNew");
                else form.Text = StiLocalization.Get("FormDatabaseEdit", "SqlCeEdit");

                form.tbName.Text = this.Name;
                form.tbAlias.Text = this.Alias;
                form.tbConnectionString.Text = this.ConnectionString;
                if (form.ShowDialog() == DialogResult.OK)
                {
                    this.Name = form.tbName.Text;
                    this.Alias = form.tbAlias.Text;
                    this.ConnectionString = form.tbConnectionString.Text;
                }
                return form.DialogResult;
            }
        }

        protected Type ConvertDbTypeToTypeInternal(string dbType)
        {
            switch (dbType.ToLowerInvariant())
            {
                case "bigint":
                case "int":
                case "uniqueidentifier":
                case "smallint":
                case "tinyint":
                    return typeof(Int64);

                case "decimal":
                case "money":
                case "smallmoney":
                    return typeof(decimal);

                case "float":
                case "real":
                    return typeof(double);

                case "datetime":
                case "smalldatetime":
                case "timestamp":
                    return typeof(DateTime);

                case "image":
                    return typeof(byte[]);
                default:
                    return typeof(string);
            }
        }
        /// <summary>
        /// Creates a new object of the type StiSqlCeDatabase.
        /// </summary>
        public StiSqlCeDatabase()
            : this(string.Empty, string.Empty)
        {
        }


        /// <summary>
        /// Creates a new object of the type StiSqlCeDatabase.
        /// </summary>
        public StiSqlCeDatabase(string name, string connectionString)
            : base(name, connectionString)
        {
        }


        /// <summary>
        /// Creates a new object of the type StiSqlCeDatabase.
        /// </summary>
        public StiSqlCeDatabase(string name, string alias, string connectionString)
            : base(name, alias, connectionString)
        {
        }

        /// <summary>
        /// Creates a new object of the type StiSqlCeDatabase.
        /// </summary>
        public StiSqlCeDatabase(string name, string alias, string connectionString, bool promptUserNameAndpassword)
            : base(name, alias, connectionString, promptUserNameAndpassword)
        {
        }
    }
}
