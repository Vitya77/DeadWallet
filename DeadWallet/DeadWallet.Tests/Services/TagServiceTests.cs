using DeadWallet.BLL.Interfaces;
using DeadWallet.BLL.Services;
using DeadWallet.DAL.Interfaces;
using DeadWallet.DAL.Models;
using DeadWallet.DAL.Repositories;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace DeadWallet.BLL.Tests.Services
{
    public class TagServiceTests
    {
        private readonly Mock<ITagRepository> _mockTagRepository;
        private readonly TagService _tagService;

        public TagServiceTests()
        {
            _mockTagRepository = new Mock<ITagRepository>();
            _tagService = new TagService(_mockTagRepository.Object);
        }

        [Fact]
        public async Task GetAllTagsAsync_ShouldReturnAllTags()
        {
            // Arrange
            var expectedTags = new List<Tag>
            {
                new Tag { Id = 1, Name = "Tag1" },
                new Tag { Id = 2, Name = "Tag2" }
            };

            _mockTagRepository.Setup(repo => repo.GetAllAsync())
                .ReturnsAsync(expectedTags);

            // Act
            var result = await _tagService.GetAllTagsAsync();

            // Assert
            Assert.Equal(expectedTags, result);
            _mockTagRepository.Verify(repo => repo.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task AddTagAsync_ShouldCallRepositoryAdd()
        {
            // Arrange
            var newTag = new Tag { Name = "NewTag" };

            _mockTagRepository.Setup(repo => repo.AddAsync(It.IsAny<Tag>()))
                .Returns(Task.CompletedTask);

            // Act
            await _tagService.AddTagAsync(newTag);

            // Assert
            _mockTagRepository.Verify(repo => repo.AddAsync(It.Is<Tag>(t => t == newTag)), Times.Once);
        }

        [Fact]
        public async Task DeleteTagAsync_ShouldCallRepositoryDeleteWithCorrectId()
        {
            // Arrange
            int tagIdToDelete = 1;

            _mockTagRepository.Setup(repo => repo.DeleteAsync(It.IsAny<int>()))
                .Returns(Task.CompletedTask);

            // Act
            await _tagService.DeleteTagAsync(tagIdToDelete);

            // Assert
            _mockTagRepository.Verify(repo => repo.DeleteAsync(It.Is<int>(id => id == tagIdToDelete)), Times.Once);
        }
    }
}