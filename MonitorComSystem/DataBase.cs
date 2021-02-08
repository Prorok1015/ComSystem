using System.Windows.Forms;
using System.Data.SQLite;
using System.IO;
using System;
using System.Globalization;
using System.Collections.Generic;

namespace MonitorComSystem
{
    class DHTDATA
    {
        public float temp { get; set; }
        public float hum { get; set; }

        public void Clear()
        {
            temp = 0;
            hum = 0;
        }

        public DHTDATA() { }
        public DHTDATA(string temp, string hum)
        {
            this.temp = Convert.ToByte(temp);
            this.hum = Convert.ToByte(hum);
        }

        override public string ToString()
        {
            return this.temp.ToString() + " " + this.hum.ToString();
        }
    }
    class Node
    {
        public string Discription { get; }
        public int Mac { get; }
        public string Type { get; }

        public Node(string Discription, int Mac, string Type)
        {
            this.Discription = Discription;
            this.Mac = Mac;
            this.Type = Type;
        }
    }

    class DataBase
    {
        public SQLiteConnection DbConn;
        private SQLiteCommand command;
        private SQLiteCommand idnodecommand;
        public DataBase(string db)
        {
           DbConn = new SQLiteConnection("Data Source="+db);
           if (!File.Exists("./" + db))
           {
               MessageBox.Show("db not found", "error", MessageBoxButtons.OK, MessageBoxIcon.Error);
           }
        }

        public void OpenConnection()
        {
            
            if (DbConn.State != System.Data.ConnectionState.Open)
            {
                
                DbConn.Open();
            }
        }

        public void CloseConnection()
        {
            if (DbConn.State != System.Data.ConnectionState.Closed)
            {
                DbConn.Close();
            }
        }

        private void InsertCommand()
        {
            command = new SQLiteCommand("INSERT INTO ms_node_data(`node_data_getdata`, `node_data_time`, `node_data_battery`, `node_data_node_id`) VALUES (@data, datetime('now','localtime'), @battary, @idnode);", DbConn);
            idnodecommand = new SQLiteCommand("SELECT id FROM ms_node WHERE Node_mac = @mac", DbConn);
        }

        public int InsertData(string Data, int Battery, int Mac)
        {
            InsertCommand();
            int IdNode = 0;
            idnodecommand.Parameters.AddWithValue("@mac", Mac);
            OpenConnection();
            SQLiteDataReader result = idnodecommand.ExecuteReader();
            if (result.HasRows)
            {
                while(result.Read())
                {
                    IdNode = Convert.ToInt32(result[0]);
                }
                command.Parameters.AddWithValue("@data", Data);
                command.Parameters.AddWithValue("@battary", Battery);
                command.Parameters.AddWithValue("@idnode", IdNode);
                int res = command.ExecuteNonQuery();
                CloseConnection();
                return res; 
            }
            CloseConnection();
            return 0;
        }

        public List<Node> SelectGateway(string MAC)
        {
            SQLiteCommand localcommand = new SQLiteCommand("SELECT node_description, node_mac, ms_type.type_name, ms_status.status_name FROM ms_node, ms_type, ms_status, ms_gateway WHERE ms_gateway.gateway_MAC = @mac AND ms_node.node_gateway_id = ms_gateway.id;", DbConn);
            localcommand.Parameters.AddWithValue("@mac", MAC);
            OpenConnection();
            SQLiteDataReader result = localcommand.ExecuteReader();
            List<Node> gateways = new List<Node>();
            if (result.HasRows)
            {
                while (result.Read())
                {
                    Node node = new Node(result[0].ToString(), Convert.ToInt32(result[1]), result[2].ToString());
                    gateways.Add(node);
                }
            }
            CloseConnection();
            return gateways;
        }
    }
}
