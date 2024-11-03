create database taassistant;

create table user_types (
  user_type_id serial primary key,
  type varchar(50) not null
);

create table users (
  user_id bigserial primary key,
  email varchar(320) not null,
  given_name varchar(50) not null,
  family_name varchar(50) not null,
  password varchar(320) not null,
  password_salt varchar(320) not null,
  cv_name varchar(320) not null,
  user_type_id serial references user_types (user_type_id),
  created_at timestamp default now()
);

create table courses (
  course_id bigserial primary key,
  prefix varchar(15) not null,
  code integer not null,
  require_ta boolean not null
);

create table messages (
  message_id bigserial primary key,
  sender_user_id bigserial references users (user_id),
  receiver_user_id bigserial references users (user_id),
  message_text text not null,
  created_at timestamp default now()
);

insert into user_types (type) values
('Admin'),
('Teaching Assistant'),
('Committee Member'),
('Instructor');


do $$
begin
end 
$$

