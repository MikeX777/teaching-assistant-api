using Azure;
using Azure.Communication.Email;
using LanguageExt;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.WindowsAzure.Storage.Blob;
using Serilog;
using System.Security.Cryptography;
using System.Text;
using TaAssistant.Interfaces.Repositories;
using TaAssistant.Model;
using TaAssistant.Model.Api;
using TaAssistant.Model.Api.Requests;
using TaAssistant.Model.Api.Responses;
using TaAssistant.Model.Entities;

namespace TaAssistant.Service.V1
{
    public record CreateUser(CreateUserRequest User) : IRequest<Either<ApiProblemDetails, CreateUserResponse>> { }
    public record SignIn(SignInRequest signIn) : IRequest<Either<ApiProblemDetails, LanguageExt.Unit>> { }
    public record Verify(VerifyRequest verify) : IRequest<Either<ApiProblemDetails, UserResponse>> { }
    public record UploadCVForUser(int UserId, IFormFile CV) : IRequest<Either<ApiProblemDetails, LanguageExt.Unit>> { }

    public class UserHandler :
        IRequestHandler<CreateUser, Either<ApiProblemDetails, CreateUserResponse>>,
        IRequestHandler<SignIn, Either<ApiProblemDetails, LanguageExt.Unit>>,
        IRequestHandler<Verify, Either<ApiProblemDetails, UserResponse>>,
        IRequestHandler<UploadCVForUser, Either<ApiProblemDetails, LanguageExt.Unit>>
    {
        ILogger log;
        IUserRepository user;
        IUserTypeRepository userType;
        Configuration configuration;
        CloudBlobContainer blob;
   
        public UserHandler(ILogger log, IUserRepository user, IUserTypeRepository userType, Configuration configuration, CloudBlobContainer blob) =>
            (this.log, this.user, this.userType, this.configuration, this.blob) = (log, user, userType, configuration, blob);

        public async Task<Either<ApiProblemDetails, CreateUserResponse>> Handle(CreateUser request, CancellationToken cancellationToken) =>
            await (
                from _1 in Common.MapLeft(() => user.UserDoesNotExist(request.User.Email)).ToAsync()
                from ut in Common.MapLeft(() => userType.GetUserTypes()).ToAsync()
                from _2 in Common.MapLeft(() => validateUserType(ut, request.User.UserTypeId)).ToAsync()
                from r in Common.MapLeft(() => updatePassword(request.User)).ToAsync()
                from u in Common.MapLeft(() => user.CreateUser(request.User, r.Item2)).ToAsync()
                select new CreateUserResponse { UserId = u}
            );

        public async Task<Either<ApiProblemDetails, LanguageExt.Unit>> Handle(SignIn request, CancellationToken cancellationToken) =>
            await (
                from u in Common.MapLeft(() => user.GetFullUser(request.signIn.Email)).ToAsync()
                from v in Common.MapLeft(() => user.SignIn(request.signIn.Email, createPasswordHash(request.signIn.Password, u.PasswordSalt, configuration.Pepper))).ToAsync()
                from _ in Common.MapLeft(() => SendEmail(v, request.signIn.Email)).ToAsync()
                select LanguageExt.Unit.Default
                );

        public async Task<Either<ApiProblemDetails, UserResponse>> Handle(Verify request, CancellationToken cancellationToken) =>
            await (
                from u in Common.MapLeft(() => user.GetFullUser(request.verify.Email)).ToAsync()
                from _ in Common.MapLeft(() => verifyEmail(request.verify.VerificationCode, u.VerificationCode, u.VerificationExpiration)).ToAsync()
                from ut in Common.MapLeft(() => userType.GetUserTypes()).ToAsync()
                select new UserResponse
                {
                    UserId = u.UserId,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    FamilyName = u.FamilyName,
                    GivenName = u.GivenName,
                    UserTypeId = u.UserTypeId,
                    UserType = ut.FirstOrDefault(e => e.UserTypeId == u.UserTypeId)?.Type ?? string.Empty
                });

        public async Task<Either<Error, LanguageExt.Unit>> SendEmail(string verificationCode, string reciepentAddress)
        {
            var emailClient = new EmailClient(configuration.EmailConnectionString);
            var message = new EmailMessage(
                senderAddress: "DoNotReply@0ab228b0-3fb2-4a4b-a83d-f5d0dd5894c1.azurecomm.net",
                content: new EmailContent("Verification Code")
                {
                    PlainText = "Verification Code",
                    Html = $@"
                    <html>
                        <body>
                            <h1>Verification code: {verificationCode}</h1>
                        </body>
                    </html>"
                },
                recipients: new EmailRecipients(new List<EmailAddress> { new EmailAddress(reciepentAddress) }));

            EmailSendOperation emailSendOperation = await emailClient.SendAsync(
                WaitUntil.Completed,
                message);
            return LanguageExt.Unit.Default;
        }

        public async Task<Either<ApiProblemDetails, LanguageExt.Unit>> Handle(UploadCVForUser request, CancellationToken cancellationToken)
        {
            try
            {

                CloudBlockBlob blockBlob = blob.GetBlockBlobReference($"cvs/{request.UserId}/cv{Path.GetExtension(request.CV.FileName)}");
                using (var stream = request.CV.OpenReadStream())
                {
                    await blockBlob.UploadFromStreamAsync(stream);
                }
                return LanguageExt.Unit.Default;
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                return ApiProblemDetails.Create("Unable to upload file", 500, ex.Message);
            }
        }

        private Either<Error, LanguageExt.Unit> verifyEmail(string suppliedVerificationCode, string storedVerificationCode, DateTimeOffset verificationExpiration) =>
            suppliedVerificationCode == storedVerificationCode && verificationExpiration > DateTimeOffset.UtcNow ?
                LanguageExt.Unit.Default :
            Error.Create(ErrorSource.UserHandler, System.Net.HttpStatusCode.BadRequest, "Verification code does not match or expired.");

        private Either<Error, LanguageExt.Unit> validateUserType(IEnumerable<UserTypeEntity> userTypes, int userTypeId) =>
            userTypes.Any(ut => ut.UserTypeId == userTypeId) ? 
                LanguageExt.Unit.Default : 
                Error.Create(
                    ErrorSource.UserHandler, 
                    System.Net.HttpStatusCode.BadRequest,
                    "User Type does not exist",
                    ErrorType.Validation);

        private Either<Error, (CreateUserRequest, string)> updatePassword(CreateUserRequest request)
        {
            var saltData = RandomNumberGenerator.GetBytes(64);
            var salt = Convert.ToBase64String(saltData);
            request.Password = createPasswordHash(request.Password, salt, configuration.Pepper);
            return (request, salt);

        }

        private string createPasswordHash(string password, string salt, string pepper) =>
            Convert.ToBase64String(SHA512.HashData(UTF8Encoding.UTF8.GetBytes(password + pepper + salt)));
    }
}
