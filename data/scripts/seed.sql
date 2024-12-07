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
  phone_number varchar(25) not null,
  password varchar(320) not null,
  password_salt varchar(320) not null,
  verification_code varchar(25) null,
  verification_expiration timestamp null,
  cv_name varchar(320) null,
  pending boolean not null,
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
  application_status_id serial references application_statuses (application_status_id),
  year int not null,
  previous_ta boolean not null,
  instructor_notes text null
);

create table courses (
  course_id serial primary key,
  prefix varchar(10) not null,
  code varchar(10) not null,
  require_ta boolean not null
);

create table grades (
  grade_id serial primary key,
  grade varchar (2) not null
);

create table application_courses (
  application_course_id bigserial primary key,
  user_id bigserial references users (user_id),
  course_id serial references courses (course_id),
  term_id serial references terms (term_id),
  year int not null,
  grade_id serial references grades (grade_id),
  application_id bigserial references applications (application_id),
  recommended boolean not null,
  selected boolean not null default FALSE
);

insert into grades (grade) values
('A'),
('B+'),
('B'),
('B-'),
('C+'),
('C'),
('C-'),
('F');

insert into courses (prefix, code, require_ta) values
('CEN', '5035', TRUE),
('STA', '4821', TRUE),
('COP', '6778', TRUE); 
  

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

