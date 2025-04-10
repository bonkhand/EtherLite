using Microsoft.AspNetCore.Mvc;
using Nethereum.Web3;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Hex.HexTypes;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace EthersLite.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BlockchainController : ControllerBase
    {
        private readonly Web3 _web3;

        public BlockchainController()
        {
            string alchemyUrl = "";
            _web3 = new Web3(alchemyUrl);
        }

        [HttpGet("wallet-info/{address}")]
        public async Task<IActionResult> GetWalletInfo(string address, [FromQuery] int limit = 1000000)
        {
            try
            {
                // Validate address
                if (!Nethereum.Util.AddressUtil.Current.IsValidEthereumAddressHexFormat(address))
                    return BadRequest("Invalid Ethereum address.");

                var txCount = await _web3.Eth.Transactions.GetTransactionCount.SendRequestAsync(address);

                // Get wallet balance
                var balanceWei = await _web3.Eth.GetBalance.SendRequestAsync(address);
                var balanceEth = Web3.Convert.FromWei(balanceWei);

                // Get current gas price
                var gasPrice = await _web3.Eth.GasPrice.SendRequestAsync();
                var gasPriceInGwei = Web3.Convert.FromWei(gasPrice, Nethereum.Util.UnitConversion.EthUnit.Gwei);

                if (txCount.Value == 0)
                {
                    return Ok(new
                    {
                        message = "This wallet has no transactions yet.",
                        walletAddress = address,
                        currentBalanceInEth = balanceEth,
                        currentGasPriceInGwei = gasPriceInGwei
                    });
                }

                var latestBlock = await _web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();

                for (var i = latestBlock.Value; i > latestBlock.Value - limit && i >= 0; i--)
                {
                    var block = await _web3.Eth.Blocks
                        .GetBlockWithTransactionsByNumber
                        .SendRequestAsync(new BlockParameter(new HexBigInteger(i)));

                    var relatedTxs = block.Transactions
                        .Where(t => t.From?.ToLower() == address.ToLower() || t.To?.ToLower() == address.ToLower())
                        .ToList();

                    if (relatedTxs.Any())
                    {
                        var lastTx = relatedTxs.Last();

                        var txBlock = await _web3.Eth.Blocks
                            .GetBlockWithTransactionsByNumber
                            .SendRequestAsync(new BlockParameter(lastTx.BlockNumber));

                        var timestamp = (long)txBlock.Timestamp.Value;
                        var txDateTime = DateTimeOffset.FromUnixTimeSeconds(timestamp).UtcDateTime;

                        return Ok(new
                        {
                            walletAddress = address,
                            currentBalanceInEth = balanceEth,
                            currentGasPriceInGwei = gasPriceInGwei,
                            scannedBlock = i,
                            lastTransaction = new
                            {
                                txHash = lastTx.TransactionHash,
                                from = lastTx.From,
                                to = lastTx.To,
                                valueInEth = Web3.Convert.FromWei(lastTx.Value),
                                gasUsed = lastTx.Gas.Value,
                                blockNumber = lastTx.BlockNumber.Value,
                                dateTimeUtc = txDateTime.ToString("yyyy-MM-dd HH:mm:ss")
                            }
                        });
                    }
                }

                return Ok(new
                {
                    message = $"No transactions found in the last {limit} blocks.",
                    walletAddress = address,
                    currentBalanceInEth = balanceEth,
                    currentGasPriceInGwei = gasPriceInGwei
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Something went wrong: {ex.Message}");
            }
        }
    }
}
