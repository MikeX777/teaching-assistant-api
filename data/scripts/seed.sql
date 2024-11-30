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

create table terms (
  term_id serial primary key,
  term_name varchar (20) not null
);

create table application_statuses (
  application_status_id serial primary key,
  status varchar (20) not null
);

create table applications (
  application_id bigserial primary key,
  user_id bigserial references users (user_id),
  term_id serial references terms (term_id),
  application_status_id serial references application_statuses (application_status_id)
  year int not null,
  previous_ta boolean not null,
  instructor_notes text null
);

insert into user_types (type) values
('Admin'),
('Teaching Assistant'),
('Committee Member'),
('Instructor');

insert into terms (type) values
('Spring'),
('Summer'),
('Fall');

insert into application_statuses (status) values
('Pending'),
('Approved'),
('Rejected'),
('Accepted'),
('Denied'),
('Completed');


do $$
begin
end 
$$

