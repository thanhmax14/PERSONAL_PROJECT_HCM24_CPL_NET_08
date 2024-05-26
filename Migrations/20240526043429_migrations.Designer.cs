﻿// <auto-generated />
using System;
using Lab01.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Lab01.Migrations
{
    [DbContext(typeof(ApplicationDBContext))]
    [Migration("20240526043429_migrations")]
    partial class migrations
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.5")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("Lab01.Models.Role", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ID"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("ID");

                    b.ToTable("roles");

                    b.HasData(
                        new
                        {
                            ID = 1,
                            Name = "User"
                        },
                        new
                        {
                            ID = 2,
                            Name = "HRM"
                        },
                        new
                        {
                            ID = 3,
                            Name = "Admin"
                        });
                });

            modelBuilder.Entity("Lab01.Models.User", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ID"));

                    b.Property<int?>("AccessFailedCount")
                        .HasColumnType("int");

                    b.Property<string>("Address")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("Birthday")
                        .HasColumnType("datetime2");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FirstName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Gender")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("IdCard")
                        .HasColumnType("int");

                    b.Property<string>("Image")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("LastName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("LockOut")
                        .HasColumnType("datetime2");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Phone")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ResetToken")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("RoleID")
                        .HasColumnType("int");

                    b.Property<string>("UserName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("UserStatus")
                        .HasColumnType("bit");

                    b.Property<DateTime>("joinin")
                        .HasColumnType("datetime2");

                    b.Property<bool>("verifyAccount")
                        .HasColumnType("bit");

                    b.HasKey("ID");

                    b.HasIndex("RoleID");

                    b.ToTable("users");

                    b.HasData(
                        new
                        {
                            ID = 1,
                            AccessFailedCount = 0,
                            Address = "Lai Vung",
                            Email = "phamquangthanhmax14@gmail.com",
                            FirstName = "Pham",
                            Gender = "Male",
                            LastName = "Quang Thanh",
                            Password = "827ccb0eea8a706c4c34a16891f84e7b",
                            Phone = "0939371017",
                            RoleID = 1,
                            UserName = "thanhmax14",
                            UserStatus = false,
                            joinin = new DateTime(2024, 5, 26, 11, 34, 27, 777, DateTimeKind.Local).AddTicks(1023),
                            verifyAccount = true
                        },
                        new
                        {
                            ID = 2,
                            AccessFailedCount = 0,
                            Address = "Lai Vung",
                            Email = "phamquangthanhmax11@gmail.com",
                            FirstName = "Le",
                            LastName = "Thi Kiwi",
                            Password = "827ccb0eea8a706c4c34a16891f84e7b",
                            Phone = "1254659899",
                            RoleID = 2,
                            UserName = "HRM",
                            UserStatus = false,
                            joinin = new DateTime(2024, 5, 26, 11, 34, 27, 777, DateTimeKind.Local).AddTicks(1037),
                            verifyAccount = true
                        },
                        new
                        {
                            ID = 3,
                            AccessFailedCount = 0,
                            Address = "Lai Vung",
                            Email = "phamquangthanhmax124@gmail.com",
                            FirstName = "Pham",
                            LastName = "Quang Thanh 1",
                            Password = "827ccb0eea8a706c4c34a16891f84e7b",
                            Phone = "454976486525",
                            RoleID = 3,
                            UserName = "admin",
                            UserStatus = false,
                            joinin = new DateTime(2024, 5, 26, 11, 34, 27, 777, DateTimeKind.Local).AddTicks(1040),
                            verifyAccount = true
                        });
                });

            modelBuilder.Entity("Lab01.Models.User", b =>
                {
                    b.HasOne("Lab01.Models.Role", "role")
                        .WithMany("users")
                        .HasForeignKey("RoleID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("role");
                });

            modelBuilder.Entity("Lab01.Models.Role", b =>
                {
                    b.Navigation("users");
                });
#pragma warning restore 612, 618
        }
    }
}
