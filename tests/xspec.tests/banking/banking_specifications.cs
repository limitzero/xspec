using System;
using xSpec;

namespace xspec.sample.tests.banking
{
	// subject under test:
	public class Account
	{
		public decimal Balance { get; private set; }

		public Account(decimal amount)
		{
			this.Balance = amount;
		}

		public void Withdraw(decimal amount)
		{
			if (this.Balance > amount)
			{
				this.Balance -= amount;
			}
			else
			{
				throw new Exception("Withdrawal exceeds current balance.");
			}
		}

		public void Deposit(decimal amount)
		{
			this.Balance += amount;
		}

		public void Transfer(decimal amount, Account toAccount)
		{
			this.Withdraw(amount);
			toAccount.Deposit(amount);
		}
		
	}

	public class banking_specifications : specification
	{
		private Account toAccount;
		private Account fromAccount;
		private decimal currentBalance;
		private decimal toAccountBalance;
		private decimal fromAccountBalance;
		private decimal transferAmount;
		
		private void when_transferring_an_amount_larger_than_the_balance_of_the_from_account()
		{
			Exception exception = null;

			before_each = () =>
							{
								fromAccount = new Account(10M);
								toAccount = new Account(50M);
							};

			establish = () =>
							{
								currentBalance = fromAccount.Balance;
								exception = catch_exception(() => { fromAccount.Transfer(15M, toAccount); });
							};
					
			it["should not allow the transfer to happen"] = () =>
			{
				currentBalance.should_be(fromAccount.Balance);
			};

			xit["should note that the withdrawal exceeds the current balance"] = () =>
			{
				"Withdrawal exceeds current balance.".should_be(exception.Message);
			};
		}

		private void when_transferring_an_amount_smaller_that_the_balance_of_the_from_account()
		{
			before_each = () =>
			{
				fromAccount = new Account(10M);
				toAccount = new Account(50M);
			};

			establish = () =>
			{
				toAccountBalance = toAccount.Balance;
				fromAccountBalance = fromAccount.Balance;
				transferAmount = 5M;
				fromAccount.Transfer(transferAmount, toAccount);
			};

			it["should withdraw the specified amount from account"] = () =>
			{
				(fromAccountBalance - transferAmount).should_be(fromAccount.Balance);
			};
			
			it["should allow the transfer to the desired account"] = () =>
			{
				var totalBalance = toAccountBalance + transferAmount;
				totalBalance.should_be(toAccount.Balance + 55); 
			};
			
		}
	}
}