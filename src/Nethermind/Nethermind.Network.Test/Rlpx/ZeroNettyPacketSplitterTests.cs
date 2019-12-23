﻿//  Copyright (c) 2018 Demerzel Solutions Limited
//  This file is part of the Nethermind library.
// 
//  The Nethermind library is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
// 
//  The Nethermind library is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//  GNU Lesser General Public License for more details.
// 
//  You should have received a copy of the GNU Lesser General Public License
//  along with the Nethermind. If not, see <http://www.gnu.org/licenses/>.

using DotNetty.Buffers;
using Nethermind.Core;
using Nethermind.Core.Extensions;
using Nethermind.Core.Test.Builders;
using Nethermind.Network.P2P.Subprotocols.Eth;
using Nethermind.Network.Rlpx;
using Nethermind.Network.Test.Rlpx.TestWrappers;
using NUnit.Framework;

namespace Nethermind.Network.Test.Rlpx
{
    [TestFixture]
    public class ZeroNettyPacketSplitterTests
    {
        private IByteBuffer _input;
        private IByteBuffer _output;

        [SetUp]
        public void Setup()
        {
            _input = PooledByteBufferAllocator.Default.Buffer(16 * 1024);
            _output = PooledByteBufferAllocator.Default.Buffer(16 * 1024);
        }
        
        [TearDown]
        public void TearDown()
        {
            _input.Release();
            _output.Release();
        }

        [TestCase(1, "000002c180000000000000000000000002000000000000000000000000000000")]
        [TestCase(2, "000400c580018204020000000000000002000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000002c280010000000000000000000000000000000000000000000000000000")]
        [TestCase(3, "000400c580018208020000000000000002000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000400c280010000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000002c280010000000000000000000000000000000000000000000000000000")]
        public void Splits_packet_into_frames(int framesCount, string outputHex)
        {
            Packet packet = new Packet("eth", 2, new byte[(framesCount - 1) * Frame.DefaultMaxFrameSize + 1]);
            _input.WriteByte(packet.PacketType);
            _input.WriteBytes(packet.Data);
            ZeroPacketSplitterTestWrapper packetSplitter = new ZeroPacketSplitterTestWrapper();
            _output = packetSplitter.Encode(_input);

            byte[] outputBytes = new byte[_output.ReadableBytes];
            _output.ReadBytes(outputBytes);

            Assert.AreEqual(outputHex, outputBytes.ToHexString(false));
        }

        [Test]
        public void Single_frame_is_handled_properly()
        {
            Packet packet = new Packet("eth", 2, new byte[Frame.DefaultMaxFrameSize / 2]);
            _input.WriteByte(packet.PacketType);
            _input.WriteBytes(packet.Data);

            ZeroPacketSplitterTestWrapper packetSplitter = new ZeroPacketSplitterTestWrapper();
            _output = packetSplitter.Encode(_input);

            byte[] outputBytes = new byte[_output.ReadableBytes];
            _output.ReadBytes(outputBytes);

            string outputHex = outputBytes.ToHexString(false);
            Assert.AreEqual("000201c1800000000000000000000000020000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", outputHex);
        }

        [Test]
        public void Block_is_handled()
        {
            Transaction a = Build.A.Transaction.TestObject;
            Transaction b = Build.A.Transaction.TestObject;
            Block block = Build.A.Block.WithTransactions(a, b).TestObject;
            NewBlockMessage newBlockMessage = new NewBlockMessage();
            newBlockMessage.Block = block;

            NewBlockMessageSerializer newBlockMessageSerializer = new NewBlockMessageSerializer();
            Packet packet = new Packet("eth", 7, newBlockMessageSerializer.Serialize(newBlockMessage));

            _input.WriteByte(packet.PacketType);
            _input.WriteBytes(packet.Data);
            ZeroPacketSplitterTestWrapper packetSplitter = new ZeroPacketSplitterTestWrapper();
            _output = packetSplitter.Encode(_input);

            byte[] outputBytes = new byte[_output.ReadableBytes];
            _output.ReadBytes(outputBytes);

            string outputHex = outputBytes.ToHexString(false);
            Assert.AreEqual("000247c180000000000000000000000007f90243f9023ff901f9a0ff483e972a04a9a62bb4b7d04ae403c615604e4090521ecc5bb7af67f71be09ca01dcc4de8dec75d7aab85b567b6ccd41ad312451b948a7413f0a142fd40d49347940000000000000000000000000000000000000000a056e81f171bcc55a6ff8345e692c0f86e5b48e01b996cadc001622fb5e363b421a056e81f171bcc55a6ff8345e692c0f86e5b48e01b996cadc001622fb5e363b421a056e81f171bcc55a6ff8345e692c0f86e5b48e01b996cadc001622fb5e363b421b9010000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000830f424080833d090080830f424083010203a02ba5557a4c62a513c7e56d1bf13373e0da6bec016755483e91589fe1c6d212e28800000000000003e8f840df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080c080000000000000000000", outputHex);
        }

        [Test]
        public void Big_block_is_handled_when_framing_enabled()
        {
            Transaction[] a = Build.A.Transaction.TestObjectNTimes(64);
            Block block = Build.A.Block.WithTransactions(a).TestObject;
            NewBlockMessage newBlockMessage = new NewBlockMessage();
            newBlockMessage.Block = block;

            NewBlockMessageSerializer newBlockMessageSerializer = new NewBlockMessageSerializer();
            Packet packet = new Packet("eth", 7, newBlockMessageSerializer.Serialize(newBlockMessage));

            _input.WriteByte(packet.PacketType);
            _input.WriteBytes(packet.Data);
            ZeroPacketSplitterTestWrapper packetSplitter = new ZeroPacketSplitterTestWrapper();
            _output = packetSplitter.Encode(_input);

            byte[] outputBytes = new byte[_output.ReadableBytes];
            _output.ReadBytes(outputBytes);

            string outputHex = outputBytes.ToHexString(false);
            TestContext.Out.WriteLine(outputHex);
            Assert.AreEqual("000400c58001820a080000000000000007f90a04f90a00f901f9a0ff483e972a04a9a62bb4b7d04ae403c615604e4090521ecc5bb7af67f71be09ca01dcc4de8dec75d7aab85b567b6ccd41ad312451b948a7413f0a142fd40d49347940000000000000000000000000000000000000000a056e81f171bcc55a6ff8345e692c0f86e5b48e01b996cadc001622fb5e363b421a056e81f171bcc55a6ff8345e692c0f86e5b48e01b996cadc001622fb5e363b421a056e81f171bcc55a6ff8345e692c0f86e5b48e01b996cadc001622fb5e363b421b9010000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000830f424080833d090080830f424083010203a02ba5557a4c62a513c7e56d1bf13373e0da6bec016755483e91589fe1c6d212e28800000000000003e8f90800df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000400c2800100000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000208c2800100000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080c0800000000000000000", outputHex);
        }

        [Test]
        public void Big_block_is_handled_when_framing_disabled()
        {
            Transaction[] a = Build.A.Transaction.TestObjectNTimes(64);
            Block block = Build.A.Block.WithTransactions(a).TestObject;
            NewBlockMessage newBlockMessage = new NewBlockMessage();
            newBlockMessage.Block = block;

            NewBlockMessageSerializer newBlockMessageSerializer = new NewBlockMessageSerializer();
            Packet packet = new Packet("eth", 7, newBlockMessageSerializer.Serialize(newBlockMessage));

            _input.WriteByte(packet.PacketType);
            _input.WriteBytes(packet.Data);
            ZeroPacketSplitterTestWrapper packetSplitter = new ZeroPacketSplitterTestWrapper();
            packetSplitter.DisableFraming();
            _output = packetSplitter.Encode(_input);

            byte[] outputBytes = new byte[_output.ReadableBytes];
            _output.ReadBytes(outputBytes);

            string outputHex = outputBytes.ToHexString(false);
            TestContext.Out.WriteLine(outputHex);
            Assert.AreEqual("000a08c180000000000000000000000007f90a04f90a00f901f9a0ff483e972a04a9a62bb4b7d04ae403c615604e4090521ecc5bb7af67f71be09ca01dcc4de8dec75d7aab85b567b6ccd41ad312451b948a7413f0a142fd40d49347940000000000000000000000000000000000000000a056e81f171bcc55a6ff8345e692c0f86e5b48e01b996cadc001622fb5e363b421a056e81f171bcc55a6ff8345e692c0f86e5b48e01b996cadc001622fb5e363b421a056e81f171bcc55a6ff8345e692c0f86e5b48e01b996cadc001622fb5e363b421b9010000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000830f424080833d090080830f424083010203a02ba5557a4c62a513c7e56d1bf13373e0da6bec016755483e91589fe1c6d212e28800000000000003e8f90800df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080df80018252089400000000000000000000000000000000000000000180808080c0800000000000000000", outputHex);
        }

        [Test]
        public void Splits_packet_into_two_frames()
        {
            Packet packet = new Packet("eth", 2, new byte[Frame.DefaultMaxFrameSize + 1]);
            _input.WriteByte(packet.PacketType);
            _input.WriteBytes(packet.Data);

            ZeroPacketSplitterTestWrapper packetSplitter = new ZeroPacketSplitterTestWrapper();
            _output = packetSplitter.Encode(_input);

            byte[] outputBytes = new byte[_output.ReadableBytes];
            _output.ReadBytes(outputBytes);

            string outputHex = outputBytes.ToHexString(false);
            Assert.AreEqual("000400c580018204020000000000000002000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000002c280010000000000000000000000000000000000000000000000000000", outputHex);
        }

        [Test]
        public void Padding_is_done_after_adding_packet_size()
        {
            Packet packet = new Packet("eth", 2, new byte[Frame.DefaultMaxFrameSize - 1]);
            _input.WriteByte(packet.PacketType);
            _input.WriteBytes(packet.Data);

            ZeroPacketSplitterTestWrapper packetSplitter = new ZeroPacketSplitterTestWrapper();
            _output = packetSplitter.Encode(_input);

            byte[] outputBytes = new byte[_output.ReadableBytes];
            _output.ReadBytes(outputBytes);

            Assert.AreEqual("000400c180000000000000000000000002000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", outputBytes.ToHexString(false));
        }
    }
}