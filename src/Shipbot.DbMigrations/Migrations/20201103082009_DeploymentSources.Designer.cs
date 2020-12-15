﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Shipbot.Data;

namespace Shipbot.DbMigrations.Migrations
{
    [DbContext(typeof(ShipbotDbContext))]
    [Migration("20201103082009_DeploymentSources")]
    partial class DeploymentSources
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .HasAnnotation("ProductVersion", "3.1.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("Shipbot.ContainerRegistry.Dao.ContainerImageMetadata", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedDateTime")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("Hash")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid>("RepositoryId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("RepositoryId");

                    b.HasIndex("RepositoryId", "Hash")
                        .IsUnique();

                    b.ToTable("containerImageMetadata");
                });

            modelBuilder.Entity("Shipbot.ContainerRegistry.Dao.ContainerImageRepository", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("Name");

                    b.ToTable("containerImageRepositories");
                });

            modelBuilder.Entity("Shipbot.ContainerRegistry.Dao.ContainerImageTag", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("MetadataId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("RepositoryId")
                        .HasColumnType("uuid");

                    b.Property<string>("Tag")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("MetadataId");

                    b.HasIndex("RepositoryId", "Tag");

                    b.ToTable("containerImageTags");
                });

            modelBuilder.Entity("Shipbot.Deployments.Dao.Deployment", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("ApplicationId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("CreationDateTime")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("CurrentImageTag")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime?>("DeploymentDateTime")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("ImageRepository")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<bool>("IsAutomaticDeployment")
                        .HasColumnType("boolean");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("TargetImageTag")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("UpdatePath")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("ApplicationId", "ImageRepository", "UpdatePath", "CurrentImageTag", "TargetImageTag")
                        .IsUnique();

                    b.ToTable("deployments");
                });

            modelBuilder.Entity("Shipbot.Deployments.Dao.DeploymentNotification", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("DeploymentId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("SlackMessageId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("DeploymentId");

                    b.HasIndex("SlackMessageId");

                    b.ToTable("deploymentNotifications");
                });

            modelBuilder.Entity("Shipbot.Deployments.Dao.DeploymentQueue", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTimeOffset?>("AcknowledgeDateTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("ApplicationId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("AttemptCount")
                        .HasColumnType("integer");

                    b.Property<DateTimeOffset>("AvailableDateTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTimeOffset>("CreationDateTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid>("DeploymentId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("DeploymentId");

                    b.HasIndex("AvailableDateTime", "AcknowledgeDateTime");

                    b.HasIndex("ApplicationId", "AvailableDateTime", "AcknowledgeDateTime");

                    b.ToTable("deploymentQueue");
                });

            modelBuilder.Entity("Shipbot.SlackIntegration.Dao.SlackMessage", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("ChannelId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTimeOffset>("CreationDateTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Timestamp")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTimeOffset>("UpdatedDateTime")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("Timestamp", "ChannelId");

                    b.ToTable("slackMessages");
                });

            modelBuilder.Entity("Shipbot.ContainerRegistry.Dao.ContainerImageMetadata", b =>
                {
                    b.HasOne("Shipbot.ContainerRegistry.Dao.ContainerImageRepository", "Repository")
                        .WithMany()
                        .HasForeignKey("RepositoryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Shipbot.ContainerRegistry.Dao.ContainerImageTag", b =>
                {
                    b.HasOne("Shipbot.ContainerRegistry.Dao.ContainerImageMetadata", "Metadata")
                        .WithMany()
                        .HasForeignKey("MetadataId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Shipbot.ContainerRegistry.Dao.ContainerImageRepository", "Repository")
                        .WithMany()
                        .HasForeignKey("RepositoryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Shipbot.Deployments.Dao.DeploymentNotification", b =>
                {
                    b.HasOne("Shipbot.Deployments.Dao.Deployment", "Deployment")
                        .WithMany()
                        .HasForeignKey("DeploymentId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Shipbot.SlackIntegration.Dao.SlackMessage", "SlackMessage")
                        .WithMany()
                        .HasForeignKey("SlackMessageId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Shipbot.Deployments.Dao.DeploymentQueue", b =>
                {
                    b.HasOne("Shipbot.Deployments.Dao.Deployment", "Deployment")
                        .WithMany()
                        .HasForeignKey("DeploymentId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}