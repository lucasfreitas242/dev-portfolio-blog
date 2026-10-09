namespace Blog.Application.Posts.PublishPost;

using Blog.Application.Exceptions;
using Blog.Domain.Interfaces;

public class PublishPostHandler(IPostRepository postRepository)
{
    public async Task<PostPublishedDto> HandleAsync(
        PublishPostCommand command, 
        CancellationToken ct = default)
    {
        var post = await postRepository.GetByIdAsync(command.Id, ct);

        if (post is null)
        {
            throw new NotFoundException("Post", command.Id);
        }

        post.Publish();
        await postRepository.UpdateAsync(post, ct);

        return new PostPublishedDto(
            Id: post.Id,
            Slug: post.Slug,
            Status: post.Status.ToString(),
            PublishedAt: post.PublishedAt
        );
    }
}
