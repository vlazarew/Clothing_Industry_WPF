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

-- Статус оплаты

insert into payment_states (Name_Of_State) values ('Не оплачено');
insert into payment_states (Name_Of_State) values ('Оплачено');

-- Тип транзакции

insert into type_of_transactions (Name_Of_Type) values ('Наличные');
insert into type_of_transactions (Name_Of_Type) values ('По карте');

-- Тип примерки

insert into types_of_fitting (Name_Of_Type) values ('Первичная');
insert into types_of_fitting (Name_Of_Type) values ('Вторичная');
insert into types_of_fitting (Name_Of_Type) values ('На дому');
insert into types_of_fitting (Name_Of_Type) values ('В офисе');

-- Поставщики

insert into suppliers (Name_Of_Supplier) values ('ИП Хорбачёв');
insert into suppliers (Name_Of_Supplier) values ('ООО БелТкань');

-- Изделия

insert into products (Name_Of_Product, Fixed_Price, MoneyToEmployee, Description, Photo)
  values('Кружевное платье', 400, 60, 'C рисунком', null);
  
-- Материалы на изделие  
insert into materials_for_product values (100001, 1, 10);
insert into materials_for_product values (100002, 1, 3);
-- Сотрудники

insert into employees values('Petrov','183461','Иван','Петров','Васильевич','89313332222','petrov_ivan@google.com','2003 200122','',null,'2019-05-05',66000,1,1);
insert into employees values('Sidorova','729461','Валентина','Сидорова','Петровна','89373232222','sidorova_valentine@google.com','2005 202322','',null,'2019-07-05',25000,1,1);
insert into employees values('admin','admin','Сергей','Кудрин','Сергеевич','89101478787','sskur@mail.ru','2005 741447','',null,'2019-01-05',35000,1,1);

-- Заказчик

insert into customers (Name,Lastname,Patronymic,Address,Phone_Number,Nickname,Birthday,Passport_data,Size,Parameters,Notes,Photo,Customer_Statuses_id_Status,Order_Channels_id_Channel,Employees_Login)
  values ('Людмила','Иванова','Ивановна','Воронеж, проспект Революции 5 кв. 11','8934222222','Ludmurik201','1991-03-07','2012 585858','44','90.60.90','Отправка СДЭКом',null,1,1,'admin');

insert into customers (Name,Lastname,Patronymic,Address,Phone_Number,Nickname,Birthday,Passport_data,Size,Parameters,Notes,Photo,Customer_Statuses_id_Status,Order_Channels_id_Channel,Employees_Login)
  values ('Виталий','Лишин','Григорьевич','Воронеж, ул. Карла Маркса д. 57','890777777','Lishai','1999-05-17','2014 999999','44','95.70.90','Дрон',null,3,4,'Sidorova');

insert into customers (Name,Lastname,Patronymic,Address,Phone_Number,Nickname,Birthday,Passport_data,Size,Parameters,Notes,Photo,Customer_Statuses_id_Status,Order_Channels_id_Channel,Employees_Login)
  values ('Борис','Бритва','Владимирович','Воронеж, проспект Лениский д. 75 кв. 81','8934222345','PeresylkaVeka','1998-11-16','2012 072497','59','190.160.95','Отправка 90-ым автобусом',null,2,3,'Petrov');

-- Заказы 

insert into orders (Date_Of_Order,Discount_Per_Cent,Total_Price,Paid,Debt,Date_Of_Delievery,Notes,Types_Of_Order_id_Type_Of_Order,Statuses_Of_Order_id_Status_Of_Order,Customers_id_Customer,Responsible,Executor, SalaryToExecutor)
  values ('2019-05-05',0,1500,300,200,'2019-06-05','',1,1,1,'admin','Petrov', 0);

insert into orders (Date_Of_Order,Discount_Per_Cent,Total_Price,Paid,Debt,Date_Of_Delievery,Notes,Types_Of_Order_id_Type_Of_Order,Statuses_Of_Order_id_Status_Of_Order,Customers_id_Customer,Responsible,Executor, SalaryToExecutor)
  values ('2018-10-15',0,4500,4500,0,'2019-05-20','',1,1,1,'admin','admin', 150);

insert into orders (Date_Of_Order,Discount_Per_Cent,Total_Price,Paid,Debt,Date_Of_Delievery,Notes,Types_Of_Order_id_Type_Of_Order,Statuses_Of_Order_id_Status_Of_Order,Customers_id_Customer,Responsible,Executor, SalaryToExecutor)
  values ('2019-04-27',0,500,300,200,'2019-06-05','',1,1,1,'Sidorova','Sidorova', 75);

-- Примерки

insert into fittings(Customers_id_Customer, Orders_id_Order, Date, Time, Notes, Types_Of_Fitting_id_Type_Of_Fitting)
	values (1, 1, '2019-06-05', '14:00:00', '', 1);
    
insert into fittings(Customers_id_Customer, Orders_id_Order, Date, Time, Notes, Types_Of_Fitting_id_Type_Of_Fitting)
	values (2, 3, '2019-05-20', '18:00:00', '', 2);
    
insert into fittings(Customers_id_Customer, Orders_id_Order, Date, Time, Notes, Types_Of_Fitting_id_Type_Of_Fitting)
	values (3, 3, '2019-06-05', '12:30:00', '', 3);

-- Документ прихода

insert into receipt_of_materials (Default_Folder,Name_Of_Document, Date_Of_Entry, Notes,Payment_States_id_Payment_States, Type_Of_Transactions_id_Type_Of_Transaction,Suppliers_id_Supplier,Total_Price)
 values ('2019','Приход №1','2019-05-05','', 1, 1, 1, 0);

-- Склад

 insert into store (Materials_Vendor_Code,Count)
 values (100001,0);
 insert into store (Materials_Vendor_Code,Count)
 values (100002,0);
