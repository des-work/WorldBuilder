using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Genisis.Infrastructure.Migrations;

[DbContext(typeof(Data.GenesisDbContext))]
[Migration("20240101000000_InitialCreate")]
partial class InitialCreate
{
    protected override void BuildTargetModel(ModelBuilder modelBuilder)
    {
#pragma warning disable 612, 618
        modelBuilder
            .HasAnnotation("ProductVersion", "8.0.0")
            .HasAnnotation("Relational:MaxIdentifierLength", 128);

        SqliteModelBuilderExtensions.UseRelationalNulls(modelBuilder);

        modelBuilder.Entity("Genisis.Core.Models.Chapter", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("INTEGER");

                b.Property<int>("ChapterOrder")
                    .HasColumnType("INTEGER");

                b.Property<string>("Content")
                    .HasMaxLength(10000)
                    .HasColumnType("TEXT");

                b.Property<DateTime>("CreatedAt")
                    .HasColumnType("TEXT");

                b.Property<int>("StoryId")
                    .HasColumnType("INTEGER");

                b.Property<string>("Title")
                    .IsRequired()
                    .HasMaxLength(200)
                    .HasColumnType("TEXT");

                b.HasKey("Id");

                b.HasIndex("StoryId");

                b.ToTable("Chapters");
            });

        modelBuilder.Entity("Genisis.Core.Models.Character", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("INTEGER");

                b.Property<string>("Bio")
                    .HasMaxLength(5000)
                    .HasColumnType("TEXT");

                b.Property<DateTime>("CreatedAt")
                    .HasColumnType("TEXT");

                b.Property<string>("Name")
                    .IsRequired()
                    .HasMaxLength(200)
                    .HasColumnType("TEXT");

                b.Property<string>("Notes")
                    .HasMaxLength(2000)
                    .HasColumnType("TEXT");

                b.Property<string>("Tier")
                    .IsRequired()
                    .HasColumnType("TEXT");

                b.Property<int>("UniverseId")
                    .HasColumnType("INTEGER");

                b.HasKey("Id");

                b.HasIndex("UniverseId");

                b.ToTable("Characters");
            });

        modelBuilder.Entity("Genisis.Core.Models.Story", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("INTEGER");

                b.Property<DateTime>("CreatedAt")
                    .HasColumnType("TEXT");

                b.Property<string>("Logline")
                    .HasMaxLength(500)
                    .HasColumnType("TEXT");

                b.Property<string>("Name")
                    .IsRequired()
                    .HasMaxLength(200)
                    .HasColumnType("TEXT");

                b.Property<int>("UniverseId")
                    .HasColumnType("INTEGER");

                b.HasKey("Id");

                b.HasIndex("UniverseId");

                b.ToTable("Stories");
            });

        modelBuilder.Entity("Genisis.Core.Models.Universe", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("INTEGER");

                b.Property<string>("Description")
                    .HasMaxLength(2000)
                    .HasColumnType("TEXT");

                b.Property<string>("Name")
                    .IsRequired()
                    .HasMaxLength(200)
                    .HasColumnType("TEXT");

                b.HasKey("Id");

                b.ToTable("Universes");
            });

        modelBuilder.Entity("Genisis.Core.Models.Chapter", b =>
            {
                b.HasOne("Genisis.Core.Models.Story", "Story")
                    .WithMany("Chapters")
                    .HasForeignKey("StoryId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.Navigation("Story");
            });

        modelBuilder.Entity("Genisis.Core.Models.Character", b =>
            {
                b.HasOne("Genisis.Core.Models.Universe", "Universe")
                    .WithMany("Characters")
                    .HasForeignKey("UniverseId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.Navigation("Universe");
            });

        modelBuilder.Entity("Genisis.Core.Models.Story", b =>
            {
                b.HasOne("Genisis.Core.Models.Universe", "Universe")
                    .WithMany("Stories")
                    .HasForeignKey("UniverseId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.Navigation("Universe");
            });

        modelBuilder.Entity("Genisis.Core.Models.Chapter", b =>
            {
                b.WithMany("Chapters")
                    .UsingEntity("ChapterCharacter",
                        j =>
                            {
                                j.IndexerProperty<int>("Id").HasColumnType("INTEGER").ValueGeneratedOnAdd();

                                j.HasOne("Genisis.Core.Models.Character").WithMany().HasForeignKey("CharactersId");

                                j.HasOne("Genisis.Core.Models.Chapter").WithMany().HasForeignKey("ChaptersId");

                                j.HasKey("ChaptersId", "CharactersId");

                                j.ToTable("ChapterCharacter");
                            });
            });

        modelBuilder.Entity("Genisis.Core.Models.Story", b =>
            {
                b.Navigation("Chapters");
            });

        modelBuilder.Entity("Genisis.Core.Models.Universe", b =>
            {
                b.Navigation("Characters");

                b.Navigation("Stories");
            });
#pragma warning restore 612, 618
    }
}
