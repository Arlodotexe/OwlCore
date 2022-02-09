using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Remoting.Transfer;

namespace OwlCore.Remoting
{
    /// <summary>
    /// Contains extension methods for easily sending and receving data.
    /// </summary>
    public static class DataProxy
    {
        /// <summary>
        /// Waits for data to be received that contains a matching <paramref name="token"/>, scoped to the given <paramref name="memberRemote"/>.
        /// </summary>
        /// <typeparam name="TResult">The type of data being received.</typeparam>
        /// <param name="memberRemote">The member remote used to receive the message.</param>
        /// <param name="token">A unique token used to identify which method call is receiving data.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the ongoing task.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation. Value is the received data.</returns>
        public static Task<TResult?> ReceiveDataAsync<TResult>(this MemberRemote memberRemote, string token, CancellationToken? cancellationToken = null)
        {
            var messageReceived = false;
            var taskCompletionSource = new TaskCompletionSource<TResult?>(cancellationToken);
            memberRemote.MessageReceived += MemberRemote_MessageReceived;

            return taskCompletionSource.Task;

            void MemberRemote_MessageReceived(object sender, IRemoteMessage e)
            {
                if (e is not RemoteDataMessage remoteDataMessage)
                    return;

                if (remoteDataMessage.Token != token)
                    return;

                if (remoteDataMessage.MemberRemoteId != memberRemote.Id)
                    return;

                // Check without concurrency
                lock (taskCompletionSource)
                {
                    // Force only receiving a message once
                    if (messageReceived)
                        return;

                    messageReceived = true;
                }

                memberRemote.MessageReceived -= MemberRemote_MessageReceived;

                var originalType = Type.GetType(remoteDataMessage.TargetMemberSignature);
                var mostDerivedType = remoteDataMessage.Result?.GetType();

                if (typeof(TResult?) != originalType)
                {
                    throw new ArgumentException($"Generic type argument does not match the received member signature. " +
                                                $"Expected {typeof(TResult?).AssemblyQualifiedName}, " +
                                                $"received ({remoteDataMessage.TargetMemberSignature}).");
                }

                if (!(remoteDataMessage.Result == null && !originalType.IsPrimitive) && !(originalType?.IsAssignableFrom(mostDerivedType) ?? false))
                {
                    if (!originalType?.IsSubclassOf(typeof(IConvertible)) ?? false)
                    {
                        throw new NotSupportedException($"Received data {mostDerivedType?.FullName ?? "null"} is not assignable from received type {originalType?.FullName ?? "null"} " +
                                                        $"and must implement {nameof(IConvertible)} for automatic type conversion. " +
                                                        $"Either handle conversion of {nameof(RemoteDataMessage)}.{nameof(RemoteDataMessage.Result)} " +
                                                        $"to this type in your {nameof(IRemoteMessageHandler.MessageConverter)} " +
                                                        $"or use a primitive type that implements {nameof(IConvertible)}.");
                    }

                    remoteDataMessage.Result = Convert.ChangeType(remoteDataMessage.Result, originalType);
                }

                taskCompletionSource.SetResult((TResult?)remoteDataMessage.Result);
            }
        }

        /// <summary>
        /// Waits the allotted <paramref name="timeToWait"/> and collected received data that contains a matching <paramref name="token"/>, scoped to the given <paramref name="memberRemote"/>.
        /// </summary>
        /// <typeparam name="TResult">The type of data being received.</typeparam>
        /// <param name="memberRemote">The member remote used to receive the message.</param>
        /// <param name="token">A unique token used to identify which method call is receiving data.</param>
        /// <param name="timeToWait">The amount of time to collect results for.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the ongoing task.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation. Value is the received data.</returns>
        public static async Task<List<TResult?>> ReceiveDataAsync<TResult>(this MemberRemote memberRemote, string token, TimeSpan timeToWait, CancellationToken? cancellationToken = null)
        {
            var results = new List<TResult?>();

            memberRemote.MessageReceived += MemberRemote_MessageReceived;

            await Task.Delay(timeToWait, cancellationToken ?? CancellationToken.None);

            memberRemote.MessageReceived -= MemberRemote_MessageReceived;

            return results;

            void MemberRemote_MessageReceived(object sender, IRemoteMessage e)
            {
                if (e is RemoteDataMessage remoteDataMessage)
                {
                    if (remoteDataMessage.Token != token)
                        return;

                    if (remoteDataMessage.MemberRemoteId != memberRemote.Id)
                        return;

                    var originalType = Type.GetType(remoteDataMessage.TargetMemberSignature);
                    var mostDerivedType = remoteDataMessage.Result?.GetType();

                    if (typeof(TResult?) != originalType)
                    {
                        throw new ArgumentException($"Generic type argument does not match the received member signature. " +
                                                    $"Expected {typeof(TResult?).AssemblyQualifiedName}, " +
                                                    $"received ({remoteDataMessage.TargetMemberSignature}).");
                    }

                    if (!(remoteDataMessage.Result == null && !originalType.IsPrimitive) && !(originalType?.IsAssignableFrom(mostDerivedType) ?? false))
                    {
                        if (!originalType?.IsSubclassOf(typeof(IConvertible)) ?? false)
                        {
                            throw new NotSupportedException($"Received data {mostDerivedType?.FullName ?? "null"} is not assignable from received type {originalType?.FullName ?? "null"} " +
                                                            $"and must implement {nameof(IConvertible)} for automatic type conversion. " +
                                                            $"Either handle conversion of {nameof(RemoteDataMessage)}.{nameof(RemoteDataMessage.Result)} " +
                                                            $"to this type in your {nameof(IRemoteMessageHandler.MessageConverter)} " +
                                                            $"or use a primitive type that implements {nameof(IConvertible)}.");
                        }

                        remoteDataMessage.Result = Convert.ChangeType(remoteDataMessage.Result, originalType);
                    }

                    results.Add((TResult?)remoteDataMessage.Result);
                }
            }
        }

        /// <summary>
        /// Publishes data remotely, to be received by <see cref="ReceiveDataAsync{TResult}(MemberRemote, string, CancellationToken?)"/>.
        /// </summary>
        /// <typeparam name="T">The type of data being sent.</typeparam>
        /// <param name="memberRemote">The member remote used to send the message.</param>
        /// <param name="data">The data to send outbound.</param>
        /// <param name="token">A unique token used to identify which method call is publishing data.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the ongoing task.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation. Value is the exact data given to <paramref name="data"/>.</returns>
        public static async Task<T> PublishDataAsync<T>(this MemberRemote memberRemote, string token, T data, CancellationToken? cancellationToken = null)
        {
            var memberSignature = MemberRemote.CreateMemberSignature(typeof(T), MemberSignatureScope.AssemblyQualifiedName);

            await memberRemote.SendRemotingMessageAsync(new RemoteDataMessage(memberRemote.Id, token, memberSignature, data), cancellationToken);
            return data;
        }
    }
}
