document.getElementById("checkWalletBtn").addEventListener("click", checkWallet);

async function checkWallet() {
    const address = document.getElementById("address").value;
    const resultDiv = document.getElementById("result");

    if (!address) {
        resultDiv.style.display = "block";
        resultDiv.className = "alert alert-warning";
        resultDiv.innerText = "Please enter a wallet address.";
        return;
    }

    try {
        const response = await fetch(`/api/blockchain/wallet-info/${address}`);
        const data = await response.json();

        resultDiv.style.display = "block";

        if (data.message) {
            resultDiv.className = "alert alert-warning";
            resultDiv.innerText = data.message;
            return;
        }

        const tx = data.lastTransaction;
        const balance = parseFloat(data.currentBalanceInEth).toFixed(5);
        const gas = parseFloat(data.currentGasPriceInGwei).toFixed(5);
        const date = tx.dateTimeUtc;

        resultDiv.className = "alert alert-info";
        resultDiv.innerHTML = `
            <h5>🧾 Wallet Summary</h5>
            <p><strong>💼 Wallet:</strong> ${data.walletAddress}</p>
            <p><strong>💰 Current Balance:</strong> ${balance} ETH</p>
            <p><strong>⛽ Current Gas Price:</strong> ${gas} Gwei</p>
            <hr>
            <h5>🔁 Last Transaction</h5>
            <p><strong>📅 Date:</strong> ${date}</p>
            <p><strong>📤 From:</strong> ${tx.from}</p>
            <p><strong>📥 To:</strong> ${tx.to}</p>
            <p><strong>💸 Amount Sent:</strong> ${parseFloat(tx.valueInEth).toFixed(5)} ETH</p>
            <p><strong>🔗 Tx Hash:</strong> ${tx.txHash}</p>
        `;
    } catch (error) {
        resultDiv.style.display = "block";
        resultDiv.className = "alert alert-danger";
        resultDiv.innerText = "Error fetching wallet info.";
        console.error(error);
    }
}
