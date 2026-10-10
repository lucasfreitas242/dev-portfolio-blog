namespace Blog.Application.Posts.DeletePost;

using Blog.Application.Exceptions;
using Blog.Domain.Enums;
using Blog.Domain.Interfaces;

public class DeletePostHandler(IPostRepository postRepository)
{
    public async Task HandleAsync(
        DeletePostCommand command, 
        CancellationToken ct = default)
    {
        var post = await postRepository.GetByIdAsync(command.Id, ct);
        if (post is null || post.Status == PostStatus.Archived)
        {
            throw new NotFoundException("Post", command.Id);
        }

        post.Archive();
        await postRepository.UpdateAsync(post, ct);
    }
}
