using Clothing_Industry_WPF.Общее.Работа_с_формами;
using Clothing_Industry_WPF.Перечисления;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Clothing_Industry_WPF.Заказы.Список_изделий_для_заказа
{
    public class OrderProducts : IDataErrorInfo
    {
        public int idOrder { get; set; }
        public string product { get; set; }
        public int count { get; set; }

        // Пустой конструктор
        public OrderProducts()
        {
            idOrder = -1;
            product = "";
            count = 0;
        }

        // Конструктор по айди (запрос в базу)
        public OrderProducts(int idOrder)
        {
            this.idOrder = idOrder;
            product = "";
            count = 0;
        }

        public List<string> TakeFreeProducts(MySqlConnection connection)
        {
            connection.Open();

            // Получение изделий, которые уже есть в этом заказе
            string query = "select Products_id_Product from list_products_to_order " +
                           "where Orders_id_Order = @idOrder";
            MySqlCommand commandCurrentProducts = new MySqlCommand(query, connection);
            commandCurrentProducts.Parameters.AddWithValue("idOrder", idOrder);
            string listOfProducts = "";

            using (DbDataReader reader = commandCurrentProducts.ExecuteReader())
            {
                while (reader.Read())
                {
                    listOfProducts += reader.GetInt32(0).ToString() + ", ";
                }
            }

            // Получаем изделия, которые еще доступны для выбора
            query = "select Name_Of_Product from products ;";

            if (listOfProducts.Length > 0)
            {
                listOfProducts = listOfProducts.Substring(0, listOfProducts.Length - 2);
                query = query.Replace(";", "where not id_Product in (" + listOfProducts + ") ; ");
            }

            MySqlCommand commandFreeProducts = new MySqlCommand(query, connection);
            List<string> result = new List<string>();

            using (DbDataReader reader = commandFreeProducts.ExecuteReader())
            {
                while (reader.Read())
                {
                    result.Add(reader.GetString(0));
                }
            }
            connection.Close();

            return result;
        }

        // Проверка на заполненность полей
        private string CheckData()
        {
            string result = "";

            if (product == "")
            {
                result += result == "" ? " Изделие" : ",  Изделие";
            }
            if (count == 0)
            {
                result += result == "" ? " Количество" : ", Количество";
            }

            return result == "" ? result : "Не заполнены обязательные поля: " + result;
        }

        // Вызов сохранения и прогон по всей логике
        public bool Save(MySqlConnection connection)
        {
            string warning = CheckData();
            if (warning == "")
            {
                connection.Open();
                MySqlTransaction transaction = connection.BeginTransaction();

                // Получение id продукта по его названию
                string queryProduct = "select id_product from products where Name_Of_Product = @name; ";
                MySqlCommand commandProduct = new MySqlCommand(queryProduct, connection);
                commandProduct.Parameters.AddWithValue("@name", product);

                int productId = -1;
                using (DbDataReader reader = commandProduct.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        productId = reader.GetInt32(0);

                    }
                }

                // Если не нашли изделие, то выходим из сохранения
                if (productId == -1)
                {
                    MessageBox.Show("Ошибка добавления", "Продукта с наименованием " + product + " не обнаружено в базе.", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }


                string query = "insert into list_products_to_order (Orders_id_Order, Products_id_Product, Count) values (@orderId, @productId, @count); ";
                MySqlCommand command = new MySqlCommand(query, connection, transaction);
                command.Parameters.AddWithValue("@orderId", idOrder);
                command.Parameters.AddWithValue("@count", count);
                command.Parameters.AddWithValue("@productId", productId);

                try
                {
                    command.ExecuteNonQuery();
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    MessageBox.Show("Ошибка добавления", "Ошибка внутри транзакции", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                /////////////////////
                connection.Close();

                // ОТКРЫТИЕ ФОРМЫ СПИСКА МАТЕРИАЛОВ ДЛЯ ИЗДЕЛИЯ (Конструктор изделия?)
                Window listMaterials = new OrderListOfMaterialsForProduct(productId);
                listMaterials.ShowDialog();
                // 

                return true;
            }
            else
            {
                MessageBox.Show(warning, "Не заполнены обязательные поля", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return false;
        }

        // Запросец на всех изделия заказа
        public static string getQueryText()
        {
            string queryText = "select id_Product, Name_Of_Product, Fixed_Price, MoneyToEmployee, Description, Count " +
                           "from products " +
                           "join list_products_to_order on list_products_to_order.Products_id_Product = products.id_Product " +
                           "where list_products_to_order.orders_id_order = @orderID " +
                           "group by Name_Of_Product, Fixed_Price, MoneyToEmployee, Description ;";

            return queryText;
        }

        // Получение данных обо всех изделиях заказа
        public static DataTable getListOrderProducts(int orderId, MySqlConnection connection)
        {
            string queryText = getQueryText();

            connection.Open();

            var parameters = new Dictionary<string, string>();
            parameters.Add("orderId", orderId.ToString());
            var dataTable = FormLoader.ExecuteQuery(queryText, connection, parameters);

            connection.Close();

            return dataTable;
        }

        // Удаление позиции из заказа
        public static void DeleteFromDB(List<int> ids, int idOrder, MySqlConnection connection)
        {
            connection.Open();

            foreach (int idProduct in ids)
            {
                MySqlTransaction transaction = connection.BeginTransaction();

                string queryTable = "delete from list_products_to_order where Products_id_Product = @id_Products and Orders_id_Order = @id_order";

                MySqlCommand commandTable = new MySqlCommand(queryTable, connection, transaction);
                commandTable.Parameters.AddWithValue("@id_Products", idProduct);
                commandTable.Parameters.AddWithValue("@id_order", idOrder);

                try
                {
                    commandTable.ExecuteNonQuery();
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    MessageBox.Show("Ошибка удаления", "Ошибка внутри транзакции", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            connection.Close();
        }

        // Здесь блок с запросом изделий и его количества
        private static Dictionary<int, int> getProductsInOrder(int idOrder, MySqlConnection connection)
        {
            string queryProduct = "select Products_id_Product, Count from list_products_to_order where Orders_id_Order = @orderId; ";
            MySqlCommand commandProduct = new MySqlCommand(queryProduct, connection);
            commandProduct.Parameters.AddWithValue("@orderId", idOrder);

            Dictionary<int, int> inOrder = new Dictionary<int, int>();
            using (DbDataReader reader = commandProduct.ExecuteReader())
            {
                while (reader.Read())
                {
                    inOrder.Add(reader.GetInt32(0), reader.GetInt32(1));
                }
            }

            return inOrder;
        }

        // Ищем материал и его количество для заказа
        private static Dictionary<string, float> getMaterialsToProductsInOrder(Dictionary<int, int> productsInOrder, MySqlConnection connection)
        {
            string productList = "";
            foreach (var keyValuePair in productsInOrder)
            {
                // Ключ - id изделия
                productList += keyValuePair.Key.ToString() + ", ";
            }

            Dictionary<string, float> inOrder = new Dictionary<string, float>();

            string queryMaterial = "select materials_for_product.Materials_Vendor_Code, materials_for_product.Count, materials_for_product.Products_id_Product " +
                                    "from materials_for_product ; ";
            if (productList.Length > 0)
            {
                productList = productList.Substring(0, productList.Length - 2);
                queryMaterial = queryMaterial.Replace(";", " where materials_for_product.Products_id_Product in (" + productList + ") ; ");
            }

            MySqlCommand commandMaterial = new MySqlCommand(queryMaterial, connection);

            using (DbDataReader reader = commandMaterial.ExecuteReader())
            {
                while (reader.Read())
                {
                    // Количество материала на изделие умножаем на кол-во изделий в заказе
                    var countMaterial = reader.GetFloat(1) * productsInOrder.Where(k => k.Key == reader.GetInt32(2)).First().Value;
                    // Есть ли данный ключ (артикул) в итоговом перечне
                    if (!productsInOrder.ContainsKey(reader.GetInt32(0)))
                    {
                        inOrder.Add(reader.GetString(0), countMaterial);
                    }
                    else
                    {
                        inOrder[reader.GetInt32(0).ToString()] += countMaterial;
                    }
                }
            }

            return inOrder;
        }

        // Хватает ли материалов для заказа
        private static (bool enough, Dictionary<string, float> needed) IsEnoughMaterialsInStore(Dictionary<string, float> materialsInOrder, MySqlConnection connection)
        {
            string vendorCodeList = "";
            foreach (var keyValuePair in materialsInOrder)
            {
                // Ключ - артикул материала
                vendorCodeList += keyValuePair.Key + ", ";
            }

            string queryStore = "select store.Count, store.Materials_Vendor_Code from store ;";

            if (vendorCodeList.Length > 0)
            {
                vendorCodeList = vendorCodeList.Substring(0, vendorCodeList.Length - 2);
                queryStore = queryStore.Replace("; ", " where store.Materials_Vendor_Code in (" + vendorCodeList + ") ; ");
            }

            MySqlCommand commandStore = new MySqlCommand(queryStore, connection);

            Dictionary<string, float> materialsInStore = new Dictionary<string, float>();

            using (DbDataReader reader = commandStore.ExecuteReader())
            {
                while (reader.Read())
                {
                    materialsInStore.Add(reader.GetString(1), reader.GetFloat(0));
                }
            }

            (bool enough, Dictionary<string, float> needed) result = (true, new Dictionary<string, float>());

            foreach (var keyValuePair in materialsInStore)
            {
                var vendorCode = keyValuePair.Key;
                var inStore = keyValuePair.Value;
                if (materialsInOrder.Where(kvp => kvp.Key == vendorCode).Count() > 0)
                {
                    var inOrder = materialsInOrder.Where(kvp => kvp.Key == vendorCode).First().Value;
                    if (inStore < inOrder)
                    {
                        result.enough = false;
                        result.needed.Add(vendorCode, inOrder - inStore);
                    }
                }
            }

            return result;
        }

        // Списание со склада
        private static void WrireOffInStore(Dictionary<string, float> materialsInOrder, MySqlConnection connection)
        {
            MySqlTransaction transaction = connection.BeginTransaction();
            // Запрос в цикле... А можно ли лучше сделать?
            foreach (var keyValuePair in materialsInOrder)
            {
                string totalQuery = "Update store set Count = Count - @CountProduct" +
                            " where Materials_Vendor_Code = @Materials_Vendor_Code;";

                MySqlCommand commandTotal = new MySqlCommand(totalQuery, connection, transaction);
                commandTotal.Parameters.AddWithValue("@Materials_Vendor_Code", keyValuePair.Key);
                commandTotal.Parameters.AddWithValue("@CountProduct", keyValuePair.Value);

                try
                {
                    commandTotal.ExecuteNonQuery();
                }
                catch
                {
                    transaction.Rollback();
                    MessageBox.Show("Ошибка добавления", "Ошибка списания со склада", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            try
            {
                transaction.Commit();
            }
            catch
            {
                MessageBox.Show("Ошибка добавления", "Ошибка внутри транзакции", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        //Отмена заказа с сообщением
        private static void CancelOrder(int idOrder, (bool enough, Dictionary<string, float> needed) delta, MySqlConnection connection)
        {
            //отменяем заказ 
            MySqlTransaction transaction = connection.BeginTransaction();
            // Ебстудей, ну и запросец. Красава!
            string queryOrder = "update orders set Statuses_Of_Order_id_Status_Of_Order = (select id_Status_Of_Order from statuses_of_order where Name_Of_Status = 'Отменён')" +
                                " where id_Order = @orderId;";
            MySqlCommand commandOrder = new MySqlCommand(queryOrder, connection, transaction);
            commandOrder.Parameters.AddWithValue("@orderId", idOrder);

            try
            {
                commandOrder.ExecuteNonQuery();
                transaction.Commit();

                // Формирование сообщения
                string vendorCodeList = "0";
                foreach (var keyValuePair in delta.needed)
                {
                    // Ключ - артикул материала
                    vendorCodeList += keyValuePair.Key + ", ";
                }

                string MessageMaterialsNeed = "";
                string queryMaterialsName = "select materials.Name_Of_Material, units.Name_Of_Unit, materials.Vendor_Code " +
                                              "from materials join units on materials.Units_id_Unit = units.id_Unit ;";

                if (vendorCodeList.Length > 0)
                {
                    vendorCodeList = vendorCodeList.Substring(0, vendorCodeList.Length - 2);
                    queryMaterialsName = queryMaterialsName.Replace("; ", "where Vendor_Code in (" + vendorCodeList + "); ");
                }

                MySqlCommand commandMaterialsName = new MySqlCommand(queryMaterialsName, connection);

                using (DbDataReader reader = commandMaterialsName.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        MessageMaterialsNeed = MessageMaterialsNeed + reader.GetString(0) + " " + delta.needed.Where(kvp => kvp.Key == reader.GetString(2)).First().Value.ToString()
                                                + " " + reader.GetString(1) + "\n";
                    }
                }
                MessageBox.Show("Материалов не хватает на изделие, заказ временно отменён!" + "\n" + "Не хватает:" + "\n" + MessageMaterialsNeed, "Недостаточно материалов на скаде", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch
            {
                transaction.Rollback();
                MessageBox.Show("Ошибка добавления", "Ошибка внутри транзакции", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Расчет необходимых материалов, списание со склада, сообщение, если чего-то не хватает
        public static bool CalcualteMaterials(int idOrder, MySqlConnection connection)
        {
            // Тут вроде все окей, но надо переделать, я ничего не понимаю
            //ОК вот блоки
            connection.Open();

            var productsinOrder = getProductsInOrder(idOrder, connection);
            var materialsToProductsInOrder = getMaterialsToProductsInOrder(productsinOrder, connection);
            var enoughMaterialsInStore = IsEnoughMaterialsInStore(materialsToProductsInOrder, connection);

            if (enoughMaterialsInStore.enough)
            {
                WrireOffInStore(materialsToProductsInOrder, connection);
            }
            else
            {
                CancelOrder(idOrder, enoughMaterialsInStore, connection);
            }

            connection.Close();

            return enoughMaterialsInStore.enough;
        }


        public string this[string columnName] => "";

        public string Error => "";
    }
}
