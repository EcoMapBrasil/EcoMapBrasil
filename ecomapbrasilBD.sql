CREATE TABLE Usuario (
id_usuario INT auto_increment primary key,
nome varchar(100) not null,
email varchar(100) not null unique,
senha varchar(255) not null,
tipo enum('normal', 'admin') default 'normal',
id_google varchar(100),
provedor_login varchar(50)
);

CREATE TABLE Avaliacao (
id_avaliacao INT auto_increment primary key,
comentario text,
nota INT CHECK (nota between 1 and 5),
data_avaliacao timestamp default current_timestamp,
id_usuario int,
foreign key (id_usuario) references Usuario(id_usuario)
);