-- Роли сотрудников

insert into employee_roles (Name_Of_Role) values('Администратор БД');
insert into employee_roles (Name_Of_Role) values('Менеджер');
insert into employee_roles (Name_Of_Role) values('Швея');

-- Должности сотрудников

insert into employee_positions (Name_Of_Position) values('Администратор');
insert into employee_positions (Name_Of_Position) values('Бухгалетер');
insert into employee_positions (Name_Of_Position) values('Помощница');

-- Статусы клиентов

insert into customer_statuses (Name_of_status) values('VIP');
insert into customer_statuses (Name_of_status) values('Обычный');
insert into customer_statuses (Name_of_status) values('Уважаемый');

-- Каналы связи

insert into order_channels (Name_of_channel) values ('Instagram');
insert into order_channels (Name_of_channel) values ('VK');
insert into order_channels (Name_of_channel) values ('Электронная почта');
insert into order_channels (Name_of_channel) values ('Рекомендация');

-- Тип заказа

insert into types_of_order (Name_Of_Type) values ('Пошив одежды');
insert into types_of_order (Name_Of_Type) values ('Переделка пошитого');

-- Статус заказа

insert into statuses_of_order (Name_of_Status) values ('Принят');
insert into statuses_of_order (Name_of_Status) values ('Готов');
insert into statuses_of_order (Name_of_Status) values ('Сдан');
insert into statuses_of_order (Name_of_Status) values ('Отправлен');
insert into statuses_of_order (Name_of_Status) values ('Отменён');

-- Единицы измерения

insert into units (Name_Of_Unit) values ('шт.');
insert into units (Name_Of_Unit) values ('м.');
insert into units (Name_Of_Unit) values ('метры погонные');

-- Группа материала

insert into groups_of_material (Name_Of_Group) values ('Материал');
insert into groups_of_material (Name_Of_Group) values ('Фурнитура');

-- Тип материала

insert into types_of_material (Name_Of_Type) values ('Однотонный');
insert into types_of_material (Name_Of_Type) values ('С рисунком');

-- Страны

INSERT into countries (Name_Of_Country) VALUES ('Россия');
insert into countries (Name_Of_Country) values ('Белоруссия');
insert into countries (Name_Of_Country) values ('Украина');

-- Материалы

insert into materials values (100001,'Синий бархат',120,'',null,3,1,1,2);
insert into materials values (100002,'Узкое кружево',50,'',null,3,1,2,1);

-- Материалы на изделие
insert into table_materials_to_product values (1,1.5,100001);
insert into table_materials_to_product values (2,0.5,100002);

-- Статус оплаты

insert into payment_states (Name_Of_State) values ('Не оплачено');
insert into payment_states (Name_Of_State) values ('Оплачено');

-- Тип транзакции

insert into type_of_transactions (Name_Of_Type) values ('Наличные');
insert into type_of_transactions (Name_Of_Type) values ('По карте');

-- Поставщики

insert into suppliers (Name_Of_Supplier) values ('ИП Хорбачёв');
insert into suppliers (Name_Of_Supplier) values ('ООО БелТкань');

-- Изделия

insert into products (Name_Of_Product, Table_Materials_To_Product_idTable_Materials_To_Product, Fixed_Price,Per_Cents,Added_Price_For_Complexity,Description,Photo)
  values('Кружевное платье',1,400,60,0,'C рисунком','');

-- Сотрудники

insert into employees values('Petrov','183461','Иван','Петров','Васильевич','89313332222','petrov_ivan@google.com','2003 200122','',null,'2019-05-05',66000,1,1);
insert into employees values('Sidorova','729461','Валентина','Сидорова','Петровна','89373232222','sidorova_valentine@google.com','2005 202322','',null,'2019-07-05',25000,1,1);
insert into employees values('admin','admin','Сергей','Кудрин','Сергеевич','89101478787','sskur@mail.ru','2005 741447','',null,'2019-01-05',35000,1,1);

-- Заказчик

insert into customers (Name,Lastname,Patronymic,Address,Phone_Number,Nickname,Birthday,Passport_data,Size,Parameters,Notes,Photo,Customer_Statuses_id_Status,Order_Channels_id_Channel,Employees_id_Employee)
  values ('Людмила','Иванова','Ивановна','Воронеж, проспект Революции 5 кв. 11','8934222222','Ludmurik201','1991-03-07','','44','90.60.90','Отправка СДЭКом','',1,1,2);

-- Заказы 

insert into orders (Date_Of_Order,Discount_Per_Cent,Paid,Debt,Date_Of_Delievery,Notes,Types_Of_Order_id_Type_Of_Order,Statuses_Of_Order_id_Status_Of_Order,Products_id_Product,Customers_id_Customer,Responsible,Executor)
  values ('2019-05-05',0,300,200,'2019-06-05','',1,1,1,1,'admin','Petrov');

-- Документ прихода

insert into documents_of_receipts (Default_Folder,Name_Of_Document, Date_Of_Entry, Amount, Price_For_One, Total_Price, Materials_Vendor_Code) values ('2019','Приход №1','2019-05-05', 15, 10, 150, 100001);
insert into documents_of_receipts (Default_Folder,Name_Of_Document, Date_Of_Entry, Amount, Price_For_One, Total_Price, Materials_Vendor_Code) values ('2018','Приход №196','2018-10-18', 20, 10, 200, 100002);
insert into documents_of_receipts (Default_Folder,Name_Of_Document, Date_Of_Entry, Amount, Price_For_One, Total_Price, Materials_Vendor_Code) values ('2019','Приход №2','2019-05-06', 15, 20, 300, 100001);

-- Поступление материалов

insert into receipt_of_materials (Documents_Of_Receipts_id_Document_Of_Receipt,Summ,Notes,Payment_States_id_Payment_States,Type_Of_Transactions_id_Type_Of_Transaction,Suppliers_id_Supplier)
values (1,200000,'Cиний бархат 30 м.п. по 250р. за 1 м.п. узкое кружево с цветочками 300 м.п. по 70 р. за 1 м.п.',1,2,1);
