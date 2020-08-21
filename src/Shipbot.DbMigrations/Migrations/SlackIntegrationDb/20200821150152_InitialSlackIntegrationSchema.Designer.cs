﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Shipbot.SlackIntegration;

namespace Shipbot.DbMigrations.Migrations.SlackIntegrationDb
{
    [DbContext(typeof(SlackIntegrationDbContext))]
    [Migration("20200821150152_InitialSlackIntegrationSchema")]
    partial class InitialSlackIntegrationSchema
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .HasAnnotation("ProductVersion", "3.1.7")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("Shipbot.SlackIntegration.Dao.DeploymentNotification", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("DeploymentId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("SlackMessageId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("SlackMessageId");

                    b.ToTable("deploymentNotifications");
                });

            modelBuilder.Entity("Shipbot.SlackIntegration.Dao.SlackMessage", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("ChannelId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("CreationDateTime")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("Timestamp")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("UpdatedDateTime")
                        .HasColumnType("timestamp without time zone");

                    b.HasKey("Id");

                    b.HasIndex("Timestamp", "ChannelId");

                    b.ToTable("slackMessages");
                });

            modelBuilder.Entity("Shipbot.SlackIntegration.Dao.DeploymentNotification", b =>
                {
                    b.HasOne("Shipbot.SlackIntegration.Dao.SlackMessage", "SlackMessage")
                        .WithMany()
                        .HasForeignKey("SlackMessageId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
