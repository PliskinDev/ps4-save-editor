﻿using System;

namespace Ionic.Zlib
{
	// Token: 0x0200013C RID: 316
	internal sealed class InflateBlocks
	{
		// Token: 0x06000C9A RID: 3226 RVA: 0x00045030 File Offset: 0x00043230
		internal InflateBlocks(ZlibCodec codec, object checkfn, int w)
		{
			this._codec = codec;
			this.hufts = new int[4320];
			this.window = new byte[w];
			this.end = w;
			this.checkfn = checkfn;
			this.mode = InflateBlocks.InflateBlockMode.TYPE;
			this.Reset();
		}

		// Token: 0x06000C9B RID: 3227 RVA: 0x000450B0 File Offset: 0x000432B0
		internal uint Reset()
		{
			uint result = this.check;
			this.mode = InflateBlocks.InflateBlockMode.TYPE;
			this.bitk = 0;
			this.bitb = 0;
			this.readAt = (this.writeAt = 0);
			if (this.checkfn != null)
			{
				this._codec._Adler32 = (this.check = Adler.Adler32(0u, null, 0, 0));
			}
			return result;
		}

		// Token: 0x06000C9C RID: 3228 RVA: 0x00045110 File Offset: 0x00043310
		internal int Process(int r)
		{
			int num = this._codec.NextIn;
			int num2 = this._codec.AvailableBytesIn;
			int num3 = this.bitb;
			int i = this.bitk;
			int num4 = this.writeAt;
			int num5 = (num4 < this.readAt) ? (this.readAt - num4 - 1) : (this.end - num4);
			int num6;
			while (true)
			{
				switch (this.mode)
				{
				case InflateBlocks.InflateBlockMode.TYPE:
					while (i < 3)
					{
						if (num2 == 0)
						{
							goto IL_96;
						}
						r = 0;
						num2--;
						num3 |= (int)(this._codec.InputBuffer[num++] & 255) << i;
						i += 8;
					}
					num6 = (num3 & 7);
					this.last = (num6 & 1);
					switch ((uint)num6 >> 1)
					{
					case 0u:
						num3 >>= 3;
						i -= 3;
						num6 = (i & 7);
						num3 >>= num6;
						i -= num6;
						this.mode = InflateBlocks.InflateBlockMode.LENS;
						continue;
					case 1u:
					{
						int[] array = new int[1];
						int[] array2 = new int[1];
						int[][] array3 = new int[1][];
						int[][] array4 = new int[1][];
						InfTree.inflate_trees_fixed(array, array2, array3, array4, this._codec);
						this.codes.Init(array[0], array2[0], array3[0], 0, array4[0], 0);
						num3 >>= 3;
						i -= 3;
						this.mode = InflateBlocks.InflateBlockMode.CODES;
						continue;
					}
					case 2u:
						num3 >>= 3;
						i -= 3;
						this.mode = InflateBlocks.InflateBlockMode.TABLE;
						continue;
					case 3u:
						goto IL_1E7;
					default:
						continue;
					}
					break;
				case InflateBlocks.InflateBlockMode.LENS:
					while (i < 32)
					{
						if (num2 == 0)
						{
							goto IL_26B;
						}
						r = 0;
						num2--;
						num3 |= (int)(this._codec.InputBuffer[num++] & 255) << i;
						i += 8;
					}
					if ((~num3 >> 16 & 65535) != (num3 & 65535))
					{
						goto Block_8;
					}
					this.left = (num3 & 65535);
					i = (num3 = 0);
					this.mode = ((this.left != 0) ? InflateBlocks.InflateBlockMode.STORED : ((this.last != 0) ? InflateBlocks.InflateBlockMode.DRY : InflateBlocks.InflateBlockMode.TYPE));
					continue;
				case InflateBlocks.InflateBlockMode.STORED:
					if (num2 == 0)
					{
						goto Block_11;
					}
					if (num5 == 0)
					{
						if (num4 == this.end && this.readAt != 0)
						{
							num4 = 0;
							num5 = ((num4 < this.readAt) ? (this.readAt - num4 - 1) : (this.end - num4));
						}
						if (num5 == 0)
						{
							this.writeAt = num4;
							r = this.Flush(r);
							num4 = this.writeAt;
							num5 = ((num4 < this.readAt) ? (this.readAt - num4 - 1) : (this.end - num4));
							if (num4 == this.end && this.readAt != 0)
							{
								num4 = 0;
								num5 = ((num4 < this.readAt) ? (this.readAt - num4 - 1) : (this.end - num4));
							}
							if (num5 == 0)
							{
								goto Block_21;
							}
						}
					}
					r = 0;
					num6 = this.left;
					if (num6 > num2)
					{
						num6 = num2;
					}
					if (num6 > num5)
					{
						num6 = num5;
					}
					Array.Copy(this._codec.InputBuffer, num, this.window, num4, num6);
					num += num6;
					num2 -= num6;
					num4 += num6;
					num5 -= num6;
					if ((this.left -= num6) == 0)
					{
						this.mode = ((this.last != 0) ? InflateBlocks.InflateBlockMode.DRY : InflateBlocks.InflateBlockMode.TYPE);
						continue;
					}
					continue;
				case InflateBlocks.InflateBlockMode.TABLE:
					while (i < 14)
					{
						if (num2 == 0)
						{
							goto IL_59C;
						}
						r = 0;
						num2--;
						num3 |= (int)(this._codec.InputBuffer[num++] & 255) << i;
						i += 8;
					}
					num6 = (this.table = (num3 & 16383));
					if ((num6 & 31) > 29 || (num6 >> 5 & 31) > 29)
					{
						goto IL_645;
					}
					num6 = 258 + (num6 & 31) + (num6 >> 5 & 31);
					if (this.blens == null || this.blens.Length < num6)
					{
						this.blens = new int[num6];
					}
					else
					{
						Array.Clear(this.blens, 0, num6);
					}
					num3 >>= 14;
					i -= 14;
					this.index = 0;
					this.mode = InflateBlocks.InflateBlockMode.BTREE;
					goto IL_7D0;
				case InflateBlocks.InflateBlockMode.BTREE:
					goto IL_7D0;
				case InflateBlocks.InflateBlockMode.DTREE:
					goto IL_8C2;
				case InflateBlocks.InflateBlockMode.CODES:
					goto IL_CD0;
				case InflateBlocks.InflateBlockMode.DRY:
					goto IL_DA9;
				case InflateBlocks.InflateBlockMode.DONE:
					goto IL_E50;
				case InflateBlocks.InflateBlockMode.BAD:
					goto IL_EAA;
				}
				break;
				IL_7D0:
				while (this.index < 4 + (this.table >> 10))
				{
					while (i < 3)
					{
						if (num2 == 0)
						{
							goto IL_71E;
						}
						r = 0;
						num2--;
						num3 |= (int)(this._codec.InputBuffer[num++] & 255) << i;
						i += 8;
					}
					this.blens[InflateBlocks.border[this.index++]] = (num3 & 7);
					num3 >>= 3;
					i -= 3;
				}
				while (this.index < 19)
				{
					this.blens[InflateBlocks.border[this.index++]] = 0;
				}
				this.bb[0] = 7;
				num6 = this.inftree.inflate_trees_bits(this.blens, this.bb, this.tb, this.hufts, this._codec);
				if (num6 != 0)
				{
					goto Block_34;
				}
				this.index = 0;
				this.mode = InflateBlocks.InflateBlockMode.DTREE;
				while (true)
				{
					IL_8C2:
					num6 = this.table;
					if (this.index >= 258 + (num6 & 31) + (num6 >> 5 & 31))
					{
						break;
					}
					num6 = this.bb[0];
					while (i < num6)
					{
						if (num2 == 0)
						{
							goto IL_8FC;
						}
						r = 0;
						num2--;
						num3 |= (int)(this._codec.InputBuffer[num++] & 255) << i;
						i += 8;
					}
					num6 = this.hufts[(this.tb[0] + (num3 & InternalInflateConstants.InflateMask[num6])) * 3 + 1];
					int num7 = this.hufts[(this.tb[0] + (num3 & InternalInflateConstants.InflateMask[num6])) * 3 + 2];
					if (num7 < 16)
					{
						num3 >>= num6;
						i -= num6;
						this.blens[this.index++] = num7;
					}
					else
					{
						int num8 = (num7 == 18) ? 7 : (num7 - 14);
						int num9 = (num7 == 18) ? 11 : 3;
						while (i < num6 + num8)
						{
							if (num2 == 0)
							{
								goto IL_A1D;
							}
							r = 0;
							num2--;
							num3 |= (int)(this._codec.InputBuffer[num++] & 255) << i;
							i += 8;
						}
						num3 >>= num6;
						i -= num6;
						num9 += (num3 & InternalInflateConstants.InflateMask[num8]);
						num3 >>= num8;
						i -= num8;
						num8 = this.index;
						num6 = this.table;
						if (num8 + num9 > 258 + (num6 & 31) + (num6 >> 5 & 31) || (num7 == 16 && num8 < 1))
						{
							goto IL_B00;
						}
						num7 = ((num7 == 16) ? this.blens[num8 - 1] : 0);
						do
						{
							this.blens[num8++] = num7;
						}
						while (--num9 != 0);
						this.index = num8;
					}
				}
				this.tb[0] = -1;
				int[] array5 = new int[]
				{
					9
				};
				int[] array6 = new int[]
				{
					6
				};
				int[] array7 = new int[1];
				int[] array8 = new int[1];
				num6 = this.table;
				num6 = this.inftree.inflate_trees_dynamic(257 + (num6 & 31), 1 + (num6 >> 5 & 31), this.blens, array5, array6, array7, array8, this.hufts, this._codec);
				if (num6 != 0)
				{
					goto Block_48;
				}
				this.codes.Init(array5[0], array6[0], this.hufts, array7[0], this.hufts, array8[0]);
				this.mode = InflateBlocks.InflateBlockMode.CODES;
				IL_CD0:
				this.bitb = num3;
				this.bitk = i;
				this._codec.AvailableBytesIn = num2;
				this._codec.TotalBytesIn += (long)(num - this._codec.NextIn);
				this._codec.NextIn = num;
				this.writeAt = num4;
				r = this.codes.Process(this, r);
				if (r != 1)
				{
					goto Block_50;
				}
				r = 0;
				num = this._codec.NextIn;
				num2 = this._codec.AvailableBytesIn;
				num3 = this.bitb;
				i = this.bitk;
				num4 = this.writeAt;
				num5 = ((num4 < this.readAt) ? (this.readAt - num4 - 1) : (this.end - num4));
				if (this.last != 0)
				{
					goto IL_DA2;
				}
				this.mode = InflateBlocks.InflateBlockMode.TYPE;
			}
			r = -2;
			this.bitb = num3;
			this.bitk = i;
			this._codec.AvailableBytesIn = num2;
			this._codec.TotalBytesIn += (long)(num - this._codec.NextIn);
			this._codec.NextIn = num;
			this.writeAt = num4;
			return this.Flush(r);
			IL_96:
			this.bitb = num3;
			this.bitk = i;
			this._codec.AvailableBytesIn = num2;
			this._codec.TotalBytesIn += (long)(num - this._codec.NextIn);
			this._codec.NextIn = num;
			this.writeAt = num4;
			return this.Flush(r);
			IL_1E7:
			num3 >>= 3;
			i -= 3;
			this.mode = InflateBlocks.InflateBlockMode.BAD;
			this._codec.Message = "invalid block type";
			r = -3;
			this.bitb = num3;
			this.bitk = i;
			this._codec.AvailableBytesIn = num2;
			this._codec.TotalBytesIn += (long)(num - this._codec.NextIn);
			this._codec.NextIn = num;
			this.writeAt = num4;
			return this.Flush(r);
			IL_26B:
			this.bitb = num3;
			this.bitk = i;
			this._codec.AvailableBytesIn = num2;
			this._codec.TotalBytesIn += (long)(num - this._codec.NextIn);
			this._codec.NextIn = num;
			this.writeAt = num4;
			return this.Flush(r);
			Block_8:
			this.mode = InflateBlocks.InflateBlockMode.BAD;
			this._codec.Message = "invalid stored block lengths";
			r = -3;
			this.bitb = num3;
			this.bitk = i;
			this._codec.AvailableBytesIn = num2;
			this._codec.TotalBytesIn += (long)(num - this._codec.NextIn);
			this._codec.NextIn = num;
			this.writeAt = num4;
			return this.Flush(r);
			Block_11:
			this.bitb = num3;
			this.bitk = i;
			this._codec.AvailableBytesIn = num2;
			this._codec.TotalBytesIn += (long)(num - this._codec.NextIn);
			this._codec.NextIn = num;
			this.writeAt = num4;
			return this.Flush(r);
			Block_21:
			this.bitb = num3;
			this.bitk = i;
			this._codec.AvailableBytesIn = num2;
			this._codec.TotalBytesIn += (long)(num - this._codec.NextIn);
			this._codec.NextIn = num;
			this.writeAt = num4;
			return this.Flush(r);
			IL_59C:
			this.bitb = num3;
			this.bitk = i;
			this._codec.AvailableBytesIn = num2;
			this._codec.TotalBytesIn += (long)(num - this._codec.NextIn);
			this._codec.NextIn = num;
			this.writeAt = num4;
			return this.Flush(r);
			IL_645:
			this.mode = InflateBlocks.InflateBlockMode.BAD;
			this._codec.Message = "too many length or distance symbols";
			r = -3;
			this.bitb = num3;
			this.bitk = i;
			this._codec.AvailableBytesIn = num2;
			this._codec.TotalBytesIn += (long)(num - this._codec.NextIn);
			this._codec.NextIn = num;
			this.writeAt = num4;
			return this.Flush(r);
			IL_71E:
			this.bitb = num3;
			this.bitk = i;
			this._codec.AvailableBytesIn = num2;
			this._codec.TotalBytesIn += (long)(num - this._codec.NextIn);
			this._codec.NextIn = num;
			this.writeAt = num4;
			return this.Flush(r);
			Block_34:
			r = num6;
			if (r == -3)
			{
				this.blens = null;
				this.mode = InflateBlocks.InflateBlockMode.BAD;
			}
			this.bitb = num3;
			this.bitk = i;
			this._codec.AvailableBytesIn = num2;
			this._codec.TotalBytesIn += (long)(num - this._codec.NextIn);
			this._codec.NextIn = num;
			this.writeAt = num4;
			return this.Flush(r);
			IL_8FC:
			this.bitb = num3;
			this.bitk = i;
			this._codec.AvailableBytesIn = num2;
			this._codec.TotalBytesIn += (long)(num - this._codec.NextIn);
			this._codec.NextIn = num;
			this.writeAt = num4;
			return this.Flush(r);
			IL_A1D:
			this.bitb = num3;
			this.bitk = i;
			this._codec.AvailableBytesIn = num2;
			this._codec.TotalBytesIn += (long)(num - this._codec.NextIn);
			this._codec.NextIn = num;
			this.writeAt = num4;
			return this.Flush(r);
			IL_B00:
			this.blens = null;
			this.mode = InflateBlocks.InflateBlockMode.BAD;
			this._codec.Message = "invalid bit length repeat";
			r = -3;
			this.bitb = num3;
			this.bitk = i;
			this._codec.AvailableBytesIn = num2;
			this._codec.TotalBytesIn += (long)(num - this._codec.NextIn);
			this._codec.NextIn = num;
			this.writeAt = num4;
			return this.Flush(r);
			Block_48:
			if (num6 == -3)
			{
				this.blens = null;
				this.mode = InflateBlocks.InflateBlockMode.BAD;
			}
			r = num6;
			this.bitb = num3;
			this.bitk = i;
			this._codec.AvailableBytesIn = num2;
			this._codec.TotalBytesIn += (long)(num - this._codec.NextIn);
			this._codec.NextIn = num;
			this.writeAt = num4;
			return this.Flush(r);
			Block_50:
			return this.Flush(r);
			IL_DA2:
			this.mode = InflateBlocks.InflateBlockMode.DRY;
			IL_DA9:
			this.writeAt = num4;
			r = this.Flush(r);
			num4 = this.writeAt;
			int arg_DE2_0 = (num4 < this.readAt) ? (this.readAt - num4 - 1) : (this.end - num4);
			if (this.readAt != this.writeAt)
			{
				this.bitb = num3;
				this.bitk = i;
				this._codec.AvailableBytesIn = num2;
				this._codec.TotalBytesIn += (long)(num - this._codec.NextIn);
				this._codec.NextIn = num;
				this.writeAt = num4;
				return this.Flush(r);
			}
			this.mode = InflateBlocks.InflateBlockMode.DONE;
			IL_E50:
			r = 1;
			this.bitb = num3;
			this.bitk = i;
			this._codec.AvailableBytesIn = num2;
			this._codec.TotalBytesIn += (long)(num - this._codec.NextIn);
			this._codec.NextIn = num;
			this.writeAt = num4;
			return this.Flush(r);
			IL_EAA:
			r = -3;
			this.bitb = num3;
			this.bitk = i;
			this._codec.AvailableBytesIn = num2;
			this._codec.TotalBytesIn += (long)(num - this._codec.NextIn);
			this._codec.NextIn = num;
			this.writeAt = num4;
			return this.Flush(r);
		}

		// Token: 0x06000C9D RID: 3229 RVA: 0x0004607C File Offset: 0x0004427C
		internal void Free()
		{
			this.Reset();
			this.window = null;
			this.hufts = null;
		}

		// Token: 0x06000C9E RID: 3230 RVA: 0x00046094 File Offset: 0x00044294
		internal void SetDictionary(byte[] d, int start, int n)
		{
			Array.Copy(d, start, this.window, 0, n);
			this.writeAt = n;
			this.readAt = n;
		}

		// Token: 0x06000C9F RID: 3231 RVA: 0x000460C0 File Offset: 0x000442C0
		internal int SyncPoint()
		{
			if (this.mode != InflateBlocks.InflateBlockMode.LENS)
			{
				return 0;
			}
			return 1;
		}

		// Token: 0x06000CA0 RID: 3232 RVA: 0x000460D0 File Offset: 0x000442D0
		internal int Flush(int r)
		{
			for (int i = 0; i < 2; i++)
			{
				int num;
				if (i == 0)
				{
					num = ((this.readAt <= this.writeAt) ? this.writeAt : this.end) - this.readAt;
				}
				else
				{
					num = this.writeAt - this.readAt;
				}
				if (num == 0)
				{
					if (r == -5)
					{
						r = 0;
					}
					return r;
				}
				if (num > this._codec.AvailableBytesOut)
				{
					num = this._codec.AvailableBytesOut;
				}
				if (num != 0 && r == -5)
				{
					r = 0;
				}
				this._codec.AvailableBytesOut -= num;
				this._codec.TotalBytesOut += (long)num;
				if (this.checkfn != null)
				{
					this._codec._Adler32 = (this.check = Adler.Adler32(this.check, this.window, this.readAt, num));
				}
				Array.Copy(this.window, this.readAt, this._codec.OutputBuffer, this._codec.NextOut, num);
				this._codec.NextOut += num;
				this.readAt += num;
				if (this.readAt == this.end && i == 0)
				{
					this.readAt = 0;
					if (this.writeAt == this.end)
					{
						this.writeAt = 0;
					}
				}
				else
				{
					i++;
				}
			}
			return r;
		}

		// Token: 0x04000692 RID: 1682
		private const int MANY = 1440;

		// Token: 0x04000693 RID: 1683
		internal static readonly int[] border = new int[]
		{
			16,
			17,
			18,
			0,
			8,
			7,
			9,
			6,
			10,
			5,
			11,
			4,
			12,
			3,
			13,
			2,
			14,
			1,
			15
		};

		// Token: 0x04000694 RID: 1684
		private InflateBlocks.InflateBlockMode mode;

		// Token: 0x04000695 RID: 1685
		internal int left;

		// Token: 0x04000696 RID: 1686
		internal int table;

		// Token: 0x04000697 RID: 1687
		internal int index;

		// Token: 0x04000698 RID: 1688
		internal int[] blens;

		// Token: 0x04000699 RID: 1689
		internal int[] bb = new int[1];

		// Token: 0x0400069A RID: 1690
		internal int[] tb = new int[1];

		// Token: 0x0400069B RID: 1691
		internal InflateCodes codes = new InflateCodes();

		// Token: 0x0400069C RID: 1692
		internal int last;

		// Token: 0x0400069D RID: 1693
		internal ZlibCodec _codec;

		// Token: 0x0400069E RID: 1694
		internal int bitk;

		// Token: 0x0400069F RID: 1695
		internal int bitb;

		// Token: 0x040006A0 RID: 1696
		internal int[] hufts;

		// Token: 0x040006A1 RID: 1697
		internal byte[] window;

		// Token: 0x040006A2 RID: 1698
		internal int end;

		// Token: 0x040006A3 RID: 1699
		internal int readAt;

		// Token: 0x040006A4 RID: 1700
		internal int writeAt;

		// Token: 0x040006A5 RID: 1701
		internal object checkfn;

		// Token: 0x040006A6 RID: 1702
		internal uint check;

		// Token: 0x040006A7 RID: 1703
		internal InfTree inftree = new InfTree();

		// Token: 0x0200013D RID: 317
		private enum InflateBlockMode
		{
			// Token: 0x040006A9 RID: 1705
			TYPE,
			// Token: 0x040006AA RID: 1706
			LENS,
			// Token: 0x040006AB RID: 1707
			STORED,
			// Token: 0x040006AC RID: 1708
			TABLE,
			// Token: 0x040006AD RID: 1709
			BTREE,
			// Token: 0x040006AE RID: 1710
			DTREE,
			// Token: 0x040006AF RID: 1711
			CODES,
			// Token: 0x040006B0 RID: 1712
			DRY,
			// Token: 0x040006B1 RID: 1713
			DONE,
			// Token: 0x040006B2 RID: 1714
			BAD
		}
	}
}
