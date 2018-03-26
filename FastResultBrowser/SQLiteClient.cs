using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;

namespace FastResultBrowser
{
    public static class SQLiteClient
    {

        // Holds our connection with the database
        private static SQLiteConnection m_dbConnection;

        static SQLiteClient()
        {
            m_dbConnection = new SQLiteConnection("Data Source=datastore.db;Version=3;");
            m_dbConnection.Open();
        }

        public static string GetSetting(string settingName)
        {
            string result;

            SQLiteCommand command = new SQLiteCommand(@"select VALUE from SETTINGS where SETTING=?;", m_dbConnection);

            var setting = new SQLiteParameter()
            {
                Value = settingName
            };

            command.Parameters.Add(setting);

            try
            {
                result = command.ExecuteScalar().ToString();
            }
            catch (System.Exception)
            {
                throw;
            }

            return result;
        }

        public static void CreateTable(string createQuery)
        {
            SQLiteCommand command = new SQLiteCommand(createQuery, m_dbConnection);

            try
            {
              command.ExecuteScalar();
            }
            catch (System.Exception)
            {
                throw;
            }
        }


        public static void InsertIntoTable(string insertQuery)
        {
            SQLiteCommand command = new SQLiteCommand(insertQuery, m_dbConnection);

            try
            {
                command.ExecuteScalar();
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        public static void DropTable(string tableName)
        {
            var query = $"DROP TABLE `{tableName}`";
            SQLiteCommand command = new SQLiteCommand(query, m_dbConnection);

            try
            {
                command.ExecuteScalar();
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        public static DataTable QueryTable(string yColumnName, string SQLQuery = "")
        {
            DataTable dt = new DataTable();
            if (string.Empty == SQLQuery)
            {
                SQLQuery = $"select Time, {yColumnName} from t1";
            }

            using (var adapter = new SQLiteDataAdapter(SQLQuery, m_dbConnection))
            {
                adapter.Fill(dt);
            }

            return dt;
        }

        public static DataColumnCollection GetTableColumns()
        {
            DataTable dt = new DataTable();

            using (var adapter = new SQLiteDataAdapter("SELECT * FROM t1 ORDER BY Time ASC LIMIT 1", m_dbConnection))
            {
                adapter.Fill(dt);
            }

            return dt.Columns;
        }

        public static int GetTableRowCount(string TableName, string ColumnName)
        {
            var query = $"select count('{ColumnName}') from {TableName};";
            var returnValue = 0;

            SQLiteCommand command = new SQLiteCommand(query, m_dbConnection);

            try
            {
                returnValue = (int) command.ExecuteScalar();
            }
            catch (System.Exception e)
            {
                throw e;
            }

            return returnValue;
        }


        #region OldCode
        /*
                public static int QueryAddUpdateOrder(List<Order> orders)
                {
                    foreach (var order in orders)
                    {
                        if (order.side != OrderSide.BUY || order.status == OrderStatus.CANCELED)
                            continue;

                        if (CheckOrderExists(order))
                        {

                        }
                        else
                        {
                            try
                            {
                                InserOrderToDatabase(order);
                            }
                            catch (System.Exception)
                            {
                                throw;
                            }
                        }
                    }


                    return 0;
                }

                public static int UpdateAssetsInDatastore( AccountInfo accountInfo )
                {
                    SQLiteCommand command = new SQLiteCommand(m_dbConnection);
                    string sql = @"delete from ASSETS;";
                    command.CommandText = sql;

                    try
                    {
                        command.ExecuteNonQuery();
                    }
                    catch (System.Exception)
                    {

                        throw;
                    }

                    sql = @"INSERT INTO `ASSETS`(`asset`,`free`,`locked`) VALUES (?,?,?);";

                    command.CommandText = sql;

                    foreach (var balance in accountInfo.balances)
                    {
                        #region SQLite Paramaters
                        var asset = new SQLiteParameter
                        {
                            Value = balance.asset
                        };

                        var free = new SQLiteParameter
                        {
                            Value = balance.free
                        };

                        var locked = new SQLiteParameter
                        {
                            Value = balance.locked
                        };
                        #endregion

                        using (command = new SQLiteCommand(sql, m_dbConnection))
                        {
                            #region Add Parameters
                            command.Parameters.Add(asset);
                            command.Parameters.Add(free);
                            command.Parameters.Add(locked);
                            #endregion

                            var result = command.ExecuteNonQuery();
                        }

                    }

                    return 0;
                }

                public static List<AssetTrackInfo> GetTrackedAssets()
                {

                    var result = new List<AssetTrackInfo>();

                    SQLiteCommand command = new SQLiteCommand(@"select * from ASSETTRACK;", m_dbConnection);

                    try
                    {
                        using (SQLiteDataReader SQLiteReader = command.ExecuteReader())
                        {
                            while (SQLiteReader.Read())
                            {

                                result.Add(new AssetTrackInfo {
                                    ATRACKID = SQLiteReader.GetInt32(0),
                                    asset = SQLiteReader.GetString(1),
                                    pairasset = SQLiteReader.GetString(2),
                                    amount = SQLiteReader.GetDecimal(3),
                                    price = SQLiteReader.GetDecimal(4)
                                });
                            }
                        }
                    }
                    catch (System.Exception)
                    {

                        throw;
                    }

                    return result;
                }

                private static bool CheckOrderExists(Order order)
                {
                    object result;
                    using (SQLiteCommand command = new SQLiteCommand(m_dbConnection))
                    {
                        command.CommandText = "select DBORDERID from ORDERS where orderID=" + order.orderId;
                        result = command.ExecuteScalar();
                    }


                    if (null == result)
                        return false;
                    else
                        return true;
                }

                private static void InserOrderToDatabase(Order order)
                {
                    string sql = @"INSERT INTO `ORDERS`(`symbol`,`orderId`,`price`,`origQty`,`executedQty`,`status`,`timeInForce`,`type`,`side`,`stopPrice`,`iceberyQty`,`time`) VALUES (?,?,?,?,?,?,?,?,?,?,?,?);";


                    #region SQLite Paramaters
                    var symbol = new SQLiteParameter
                    {
                        Value = order.symbol
                    };

                    var orderId = new SQLiteParameter
                    {
                        Value = order.orderId
                    };

                    var price = new SQLiteParameter
                    {
                        Value = order.price
                    };

                    var origQty = new SQLiteParameter
                    {
                        Value = order.origQty
                    };

                    var executedQty = new SQLiteParameter
                    {
                        Value = order.executedQty
                    };

                    var status = new SQLiteParameter
                    {
                        Value = order.status
                    };

                    var timeInForce = new SQLiteParameter
                    {
                        Value = order.timeInForce
                    };

                    var type = new SQLiteParameter
                    {
                        Value = order.type
                    };

                    var side = new SQLiteParameter
                    {
                        Value = order.side
                    };

                    var stopPrice = new SQLiteParameter
                    {
                        Value = order.stopPrice
                    };

                    var icebergQty = new SQLiteParameter
                    {
                        Value = order.icebergQty
                    };

                    var time = new SQLiteParameter
                    {
                        Value = order.time
                    };
                    #endregion

                    using (SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection))
                    {
                        #region Add Parameters
                        command.Parameters.Add(symbol);
                        command.Parameters.Add(orderId);
                        command.Parameters.Add(price);
                        command.Parameters.Add(origQty);
                        command.Parameters.Add(executedQty);
                        command.Parameters.Add(status);
                        command.Parameters.Add(timeInForce);
                        command.Parameters.Add(type);
                        command.Parameters.Add(side);
                        command.Parameters.Add(stopPrice);
                        command.Parameters.Add(icebergQty);
                        command.Parameters.Add(time);
                        #endregion

                        var result = command.ExecuteNonQuery();
                    }
                }
        */
        #endregion

    }
}
