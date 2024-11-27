using Azure;
using Azure.Communication.Email;
using LanguageExt;
using MediatR;
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
    public record CreateUser(CreateUserRequest User) : IRequest<Either<ApiProblemDetails, LanguageExt.Unit>> { }
    public record SignIn(SignInRequest signIn) : IRequest<Either<ApiProblemDetails, LanguageExt.Unit>> { }
    public record Verify(VerifyRequest verify) : IRequest<Either<ApiProblemDetails, UserResponse>> { }

    public class UserHandler :
        IRequestHandler<CreateUser, Either<ApiProblemDetails, LanguageExt.Unit>>,
        IRequestHandler<SignIn, Either<ApiProblemDetails, LanguageExt.Unit>>,
        IRequestHandler<Verify, Either<ApiProblemDetails, UserResponse>>
    {
        IUserRepository user;
        IUserTypeRepository userType;
        Configuration configuration;
   
        public UserHandler(IUserRepository user, IUserTypeRepository userType, Configuration configuration) =>
            (this.user, this.userType, this.configuration) = (user, userType, configuration);

        public async Task<Either<ApiProblemDetails, LanguageExt.Unit>> Handle(CreateUser request, CancellationToken cancellationToken) =>
            await (
                from ut in Common.MapLeft(() => userType.GetUserTypes()).ToAsync()
                from _ in Common.MapLeft(() => validateUserType(ut, request.User.UserTypeId)).ToAsync()
                from r in Common.MapLeft(() => updatePassword(request.User)).ToAsync()
                from u in Common.MapLeft(() => user.CreateUser(request.User)).ToAsync()
                select LanguageExt.Unit.Default
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
                    Email = u.Email,
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

        private Either<Error, CreateUserRequest> updatePassword(CreateUserRequest request)
        {
            var saltData = RandomNumberGenerator.GetBytes(64);
            request.PasswordSalt = Convert.ToBase64String(saltData);
            request.Password = createPasswordHash(request.Password, request.PasswordSalt, configuration.Pepper);
            return request;

        }

        private string createPasswordHash(string password, string salt, string pepper) =>
            Convert.ToBase64String(SHA512.HashData(UTF8Encoding.UTF8.GetBytes(password + pepper + salt)));
    }
}
