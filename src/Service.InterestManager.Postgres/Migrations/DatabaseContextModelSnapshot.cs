﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Service.InterestManager.Postrges;

namespace Service.InterestManager.Postrges.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    partial class DatabaseContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("interest_manager")
                .HasAnnotation("Relational:MaxIdentifierLength", 63)
                .HasAnnotation("ProductVersion", "5.0.11")
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            modelBuilder.Entity("Service.IntrestManager.Domain.Models.CalculationHistory", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<double>("AmountInWalletsInUsd")
                        .HasColumnType("double precision");

                    b.Property<double>("CalculatedAmountInUsd")
                        .HasColumnType("double precision");

                    b.Property<DateTime>("CalculationDate")
                        .HasColumnType("timestamp without time zone");

                    b.Property<DateTime>("CompletedDate")
                        .HasColumnType("timestamp without time zone");

                    b.Property<DateTime>("LastTs")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("SettingsJson")
                        .HasColumnType("text");

                    b.Property<int>("WalletCount")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("CompletedDate");

                    b.HasIndex("LastTs");

                    b.ToTable("calculationhistory");
                });

            modelBuilder.Entity("Service.IntrestManager.Domain.Models.IndexPriceEntity", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("Asset")
                        .HasMaxLength(64)
                        .HasColumnType("character varying(64)");

                    b.Property<decimal>("PriceInUsd")
                        .HasColumnType("numeric");

                    b.HasKey("Id");

                    b.HasIndex("Asset");

                    b.ToTable("indexprice");
                });

            modelBuilder.Entity("Service.IntrestManager.Domain.Models.InterestRateCalculation", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<decimal>("Amount")
                        .HasColumnType("numeric");

                    b.Property<decimal>("Apr")
                        .HasColumnType("numeric");

                    b.Property<DateTime>("Date")
                        .HasColumnType("timestamp without time zone");

                    b.Property<long>("HistoryId")
                        .HasColumnType("bigint");

                    b.Property<DateTime>("LastTs")
                        .HasColumnType("timestamp without time zone");

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

                    b.ToTable("interestratecalculation");
                });

            modelBuilder.Entity("Service.IntrestManager.Domain.Models.InterestRatePaid", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<decimal>("Amount")
                        .HasColumnType("numeric");

                    b.Property<DateTime>("Date")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("ErrorMessage")
                        .HasColumnType("text");

                    b.Property<long>("HistoryId")
                        .HasColumnType("bigint");

                    b.Property<DateTime>("LastTs")
                        .HasColumnType("timestamp without time zone");

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

                    b.ToTable("interestratepaid");
                });

            modelBuilder.Entity("Service.IntrestManager.Domain.Models.InterestRateSettings", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

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
                        .HasColumnType("timestamp without time zone");

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

                    b.ToTable("interestratesettings");
                });

            modelBuilder.Entity("Service.IntrestManager.Domain.Models.PaidHistory", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("timestamp without time zone");

                    b.Property<DateTime>("LastTs")
                        .HasColumnType("timestamp without time zone");

                    b.Property<DateTime>("RangeFrom")
                        .HasColumnType("timestamp without time zone");

                    b.Property<DateTime>("RangeTo")
                        .HasColumnType("timestamp without time zone");

                    b.Property<double>("TotalPaidInUsd")
                        .HasColumnType("double precision");

                    b.Property<int>("WalletCount")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("CreatedDate");

                    b.HasIndex("LastTs");

                    b.ToTable("paidhistory");
                });
#pragma warning restore 612, 618
        }
    }
}
