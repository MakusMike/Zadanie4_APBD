using System.Data.SqlClient;
using System.Data;


namespace Zadanie4_APBD;

public class DataService : IDataService
{
     private const string _connect =
            "Data Source=db-mssql16.pjwstk.edu.pl;Initial Catalog=s25713;Integrated Security=True";


     public async Task<int> InsertProduct(Order order)
     {
         var x = 0;

         using var connection = new SqlConnection(_connect);
         using var con =
             new SqlCommand("select count (*) idProductCount from Product where IdProduct = @IdProduct", connection);

         con.Parameters.AddWithValue("idProduct", order.IdProduct);
         await connection.OpenAsync();
         await con.ExecuteNonQueryAsync();

         using (var dr = await con.ExecuteReaderAsync())
         {
             while (await dr.ReadAsync())
             {

                 x = int.Parse(dr["idProductCount"].ToString());

             }
         }

         using (var connection1 = new SqlConnection(_connect))
         {
             await connection1.OpenAsync();
             using (SqlTransaction transaction = connection1.BeginTransaction())
             {
                 try
                 {
                     using (var com = new SqlCommand())
                     {
                         com.Connection = connection1;
                         com.Transaction = transaction;
                         com.CommandText = "AddProductToWarehouse";
                         com.CommandType = CommandType.StoredProcedure;

                         com.Parameters.AddWithValue("@IdProduct", order.IdProduct);
                         com.Parameters.AddWithValue("@IdWarehouse", order.IdWarehouse);
                         com.Parameters.AddWithValue("@Amount", order.Amount);
                         com.Parameters.AddWithValue("@CreatedAt", order.CreatedAt);

                         await com.ExecuteNonQueryAsync();
                         await transaction.CommitAsync();
                         await connection1.CloseAsync();
                     }
                 }catch (SqlException)
                 {
                        await transaction.RollbackAsync();
                 }

             }
         }


         con.Parameters.Clear(); 
         con.CommandText = 
                    "select count (*) IdWarehouseCount from Warehouse where IdWarehouse = @idWarehouse";
         con.Parameters.AddWithValue("@Idwarehouse", order.IdWarehouse);               
         await con.ExecuteNonQueryAsync();
               
            var y = 0;
                using (var dr = await con.ExecuteReaderAsync())
                {

                    while (dr.Read())
                    {

                        y = int.Parse(dr["IdWarehouseCount"].ToString());

                    }
                }

                if(x == 0 || y == 0 || order.Amount == 0)
                {
                    return -1;                    
                }

                var booled = 0;


                con.Parameters.Clear();
                con.CommandText = 
                    "select count (*) booled from [Order] o where o.IdProduct = @idProduct and o.Amount = @amount and o.date < @date and not exists (select p.IdOrder from Product_Warehouse p  where p.IdOrder = o.IdOrder) ";
                con.Parameters.AddWithValue("@idWarehouse", order.IdWarehouse);
                con.Parameters.AddWithValue("@amount", order.Amount);
                con.Parameters.AddWithValue("@date", order.CreatedAt);
                await con.ExecuteNonQueryAsync();

                using (var dr = await con.ExecuteReaderAsync())
                {
                    while(await dr.ReadAsync())
                    {
                        booled = int.Parse(dr["booled"].ToString());
                    }
                }


                if (booled == 0)
                {
                    return -2;
                }

                var r = await con.ExecuteNonQueryAsync();

                con.Parameters.Clear();
                con.CommandText = 
                    "update [Order] set FullfilledAt = @date where IdWarehouse = @idWarehouse and Amount = @amount";
                con.Parameters.AddWithValue("@date", DateTime.Now.ToString());
                con.Parameters.AddWithValue("@idWarehouse", order.IdWarehouse);
                con.Parameters.AddWithValue("@amount", order.Amount);
                

                    await con.ExecuteNonQueryAsync();

                con.Parameters.Clear();
                con.CommandText = 
                    "insert into Product_Warehouse values(@idWarehouse, @idProduct,(select IdOrder from [Order] where IdProduct = @idProduct and Amount = @amount), @Amount,((select Price from Product where IdProduct = @idProduct) *  @amount) , @date )";
                con.Parameters.AddWithValue("@idWarehouse", order.IdWarehouse);
                con.Parameters.AddWithValue("@amount", order.Amount);
                con.Parameters.AddWithValue("@idProduct", order.IdProduct);
                con.Parameters.AddWithValue("@date", DateTime.Now.ToString());

                    await con.ExecuteNonQueryAsync();

                con.Parameters.Clear();
                con.CommandText = 
                    "select IdProductWarehouse from Product_Warehouse where IdOrder = ( select IdOrder from [Order] where IdProduct = @idProduct and Amount = @amount)";
                con.Parameters.AddWithValue("@amount", order.Amount);
                con.Parameters.AddWithValue("@idProduct", order.IdProduct);

                    await con.ExecuteNonQueryAsync();

            var z = 0;
            using (var dr = await con.ExecuteReaderAsync())
            {
                    while (await dr.ReadAsync())
                    {
                        z = int.Parse(dr["IdProductWarehouse"].ToString());
                    }
            }
            await connection.CloseAsync();
            return z;
        }
            
        public async void Insert(Order order)
        {
            await using var connection = new SqlConnection(_connect);
            await connection.OpenAsync();
            await using var command = new SqlCommand("AddProductToWarehouse", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@idProduct", order.IdProduct);
            command.Parameters.AddWithValue("@idWarehouse", order.IdWarehouse);
            command.Parameters.AddWithValue("@Amount", order.Amount);
            command.Parameters.AddWithValue("@date", DateTime.Now.ToString());
            await command.ExecuteNonQueryAsync();

            connection.Close();
        }
}