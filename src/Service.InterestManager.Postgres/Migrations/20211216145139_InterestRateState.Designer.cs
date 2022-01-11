﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Service.InterestManager.Postrges;

#nullable disable

namespace Service.InterestManager.Postrges.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    [Migration("20211216145139_InterestRateState")]
    partial class InterestRateState
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("interest_manager")
                .HasAnnotation("ProductVersion", "6.0.1")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Service.IntrestManager.Domain.Models.CalculationHistory", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<double>("AmountInWalletsInUsd")
                        .HasColumnType("double precision");

                    b.Property<double>("CalculatedAmountInUsd")
                        .HasColumnType("double precision");

                    b.Property<DateTime>("CalculationDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime>("CompletedDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime>("LastTs")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("SettingsJson")
                        .HasColumnType("text");

                    b.Property<int>("WalletCount")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("CompletedDate");

                    b.HasIndex("LastTs");

                    b.ToTable("calculationhistory", "interest_manager");
                });

            modelBuilder.Entity("Service.IntrestManager.Domain.Models.IndexPriceEntity", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<string>("Asset")
                        .HasMaxLength(64)
                        .HasColumnType("character varying(64)");

                    b.Property<decimal>("PriceInUsd")
                        .HasColumnType("numeric");

                    b.HasKey("Id");

                    b.HasIndex("Asset");

                    b.ToTable("indexprice", "interest_manager");
                });

            modelBuilder.Entity("Service.IntrestManager.Domain.Models.InterestRateCalculation", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<decimal>("Amount")
                        .HasColumnType("numeric");

                    b.Property<decimal>("Apr")
                        .HasColumnType("numeric");

                    b.Property<DateTime>("Date")
                        .HasColumnType("timestamp with time zone");

                    b.Property<long>("HistoryId")
                        .HasColumnType("bigint");

                    b.Property<DateTime>("LastTs")
                        .HasColumnType("timestamp with time zone");

                    b.Property<decimal>("NewBalance")
                        .HasColumnType("numeric");

                    b.Property<string>("Symbol")
                        .HasMaxLength(64)
                        .HasColumnType("character varying(64)");

                    b.Property<string>("WalletId")
                        .HasMaxLength(64)
                        .HasColumnType("character varying(64)");

                    b.HasKey("Id");

                    b.HasIndex("LastTs");

                    b.HasIndex("Symbol");

                    b.HasIndex("WalletId");

                    b.HasIndex("WalletId", "Symbol");

                    b.HasIndex("WalletId", "Symbol", "Date")
                        .IsUnique();

                    b.ToTable("interestratecalculation", "interest_manager");
                });

            modelBuilder.Entity("Service.IntrestManager.Domain.Models.InterestRatePaid", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<decimal>("Amount")
                        .HasColumnType("numeric");

                    b.Property<DateTime>("Date")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("ErrorMessage")
                        .HasColumnType("text");

                    b.Property<long>("HistoryId")
                        .HasColumnType("bigint");

                    b.Property<DateTime>("LastTs")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("State")
                        .HasColumnType("integer");

                    b.Property<string>("Symbol")
                        .HasMaxLength(64)
                        .HasColumnType("character varying(64)");

                    b.Property<string>("TransactionId")
                        .HasColumnType("text");

                    b.Property<string>("WalletId")
                        .HasMaxLength(64)
                        .HasColumnType("character varying(64)");

                    b.HasKey("Id");

                    b.HasIndex("LastTs");

                    b.HasIndex("State");

                    b.HasIndex("Symbol");

                    b.HasIndex("WalletId");

                    b.HasIndex("WalletId", "Symbol");

                    b.HasIndex("WalletId", "Symbol", "Date")
                        .IsUnique();

                    b.ToTable("interestratepaid", "interest_manager");
                });

            modelBuilder.Entity("Service.IntrestManager.Domain.Models.InterestRateSettings", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<decimal>("Apr")
                        .HasColumnType("numeric");

                    b.Property<decimal>("Apy")
                        .HasColumnType("numeric");

                    b.Property<string>("Asset")
                        .HasMaxLength(64)
                        .HasColumnType("character varying(64)");

                    b.Property<decimal>("DailyLimitInUsd")
                        .HasColumnType("numeric");

                    b.Property<DateTime>("LastTs")
                        .HasColumnType("timestamp with time zone");

                    b.Property<decimal>("RangeFrom")
                        .HasColumnType("numeric");

                    b.Property<decimal>("RangeTo")
                        .HasColumnType("numeric");

                    b.Property<string>("WalletId")
                        .HasMaxLength(64)
                        .HasColumnType("character varying(64)");

                    b.HasKey("Id");

                    b.HasIndex("Asset");

                    b.HasIndex("LastTs");

                    b.HasIndex("WalletId");

                    b.HasIndex("WalletId", "Asset");

                    b.HasIndex("WalletId", "Asset", "RangeFrom", "RangeTo")
                        .IsUnique();

                    b.ToTable("interestratesettings", "interest_manager");
                });

            modelBuilder.Entity("Service.IntrestManager.Domain.Models.InterestRateState", b =>
                {
                    b.Property<string>("AssetId")
                        .HasColumnType("text");

                    b.Property<string>("WalletId")
                        .HasColumnType("text");

                    b.Property<decimal>("CurrentEarnAmount")
                        .HasColumnType("numeric");

                    b.Property<decimal>("TotalEarnAmount")
                        .HasColumnType("numeric");

                    b.HasKey("AssetId", "WalletId");

                    b.ToTable("interestratestate", "interest_manager");
                });

            modelBuilder.Entity("Service.IntrestManager.Domain.Models.PaidHistory", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime>("LastTs")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime>("RangeFrom")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime>("RangeTo")
                        .HasColumnType("timestamp with time zone");

                    b.Property<double>("TotalPaidInUsd")
                        .HasColumnType("double precision");

                    b.Property<int>("WalletCount")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("CreatedDate");

                    b.HasIndex("LastTs");

                    b.ToTable("paidhistory", "interest_manager");
                });
#pragma warning restore 612, 618
        }
    }
}