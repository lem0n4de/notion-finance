﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NotionFinance.Data;

#nullable disable

namespace NotionFinance.Migrations
{
    [DbContext(typeof(UserDbContext))]
    [Migration("20220112143814_InitialMigration")]
    partial class InitialMigration
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 1L, 1);

            modelBuilder.Entity("NotionFinance.Models.Account", b =>
                {
                    b.Property<long>("AccountId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("AccountId"), 1L, 1);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<long?>("UserId")
                        .HasColumnType("bigint");

                    b.HasKey("AccountId");

                    b.HasIndex("UserId");

                    b.ToTable("Account");
                });

            modelBuilder.Entity("NotionFinance.Models.NotionUserSettings", b =>
                {
                    b.Property<long>("NotionUserSettingsId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("NotionUserSettingsId"), 1L, 1);

                    b.Property<string>("AuthorizedWorkspaceId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("HomePageName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("MasterDatabaseExists")
                        .HasColumnType("bit");

                    b.Property<string>("MasterDatabaseName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("NotionAccessToken")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("NotionId")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("NotionUserSettingsId");

                    b.ToTable("NotionUserSettings");
                });

            modelBuilder.Entity("NotionFinance.Models.Transaction", b =>
                {
                    b.Property<int>("TransactionId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("TransactionId"), 1L, 1);

                    b.Property<int>("AccountId")
                        .HasColumnType("int");

                    b.Property<long>("AccountId1")
                        .HasColumnType("bigint");

                    b.Property<string>("Asset")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Position")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<float>("Price")
                        .HasColumnType("real");

                    b.Property<string>("Ticker")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.Property<long>("UserId1")
                        .HasColumnType("bigint");

                    b.HasKey("TransactionId");

                    b.HasIndex("AccountId1");

                    b.HasIndex("UserId1");

                    b.ToTable("Transaction");
                });

            modelBuilder.Entity("NotionFinance.Models.User", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("Id"), 1L, 1);

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FirstName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsAdmin")
                        .HasColumnType("bit");

                    b.Property<string>("LastName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Membership")
                        .HasColumnType("int");

                    b.Property<long>("NotionUserSettingsId")
                        .HasColumnType("bigint");

                    b.Property<string>("PasswordHash")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<byte[]>("PasswordSalt")
                        .IsRequired()
                        .HasColumnType("varbinary(max)");

                    b.HasKey("Id");

                    b.HasIndex("NotionUserSettingsId");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("NotionFinance.Models.Account", b =>
                {
                    b.HasOne("NotionFinance.Models.User", null)
                        .WithMany("Accounts")
                        .HasForeignKey("UserId");
                });

            modelBuilder.Entity("NotionFinance.Models.Transaction", b =>
                {
                    b.HasOne("NotionFinance.Models.Account", "Account")
                        .WithMany("Transactions")
                        .HasForeignKey("AccountId1")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("NotionFinance.Models.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId1")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Account");

                    b.Navigation("User");
                });

            modelBuilder.Entity("NotionFinance.Models.User", b =>
                {
                    b.HasOne("NotionFinance.Models.NotionUserSettings", "NotionUserSettings")
                        .WithMany()
                        .HasForeignKey("NotionUserSettingsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("NotionUserSettings");
                });

            modelBuilder.Entity("NotionFinance.Models.Account", b =>
                {
                    b.Navigation("Transactions");
                });

            modelBuilder.Entity("NotionFinance.Models.User", b =>
                {
                    b.Navigation("Accounts");
                });
#pragma warning restore 612, 618
        }
    }
}
