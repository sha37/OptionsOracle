# OptionsOracle
# Option Oracle

Option Oracle is an open-source, advanced options trading and analysis tool that offers traders a comprehensive platform to 
analyze, strategize, and execute options trades. It is designed to help traders understand the options market, visualize potential outcomes, 
and find optimal trading strategies.

## Features

- Supports multiple options trading strategies
- Real-time market data and historical data analysis
- In-depth options chain visualization
- Advanced Greeks calculations and risk analysis
- Portfolio management and tracking
- Customizable user interface

## Getting Started

# OptionsOracle v1.7.4(Latest)

## What's New?

* Implemented stock lookup functionality for NSE. This allows users to efficiently search and select from a wide range of stocks listed on the NSE.

## Previous Updates:

##v1.7.3

- Implemented display of option lot size.

##v1.7.2

### Dynamic Stock List Appending

-We have made a significant upgrade to the handling of our stock lists. Now, the stock list is dynamically appended to the stock text depending on the plugin and its resources. This feature has currently been implemented for NSE (National Stock Exchange) only, but we are looking to expand it to other exchanges in future updates.
-introduced a new stock template in JSON format for NSE. This will make it easier for users to work with NSE stocks and will provide a more standardized format for our stock data.

##v1.7.1	
-Implemented strike combo box to filter the option grid view.
-Alter position and size of various attributes to get better view experience.

##v1.7.0
-Removed extra side banners for a more streamlined and spacious viewing experience.
-Unified our XML structures to improve data management efficiency.
-Enhanced the loading form to make data loading smoother and quicker.

##v1.0.8
-Implemented GetHistoricalData for NSE (National Stock Exchange) using Yahoo Finance.
-Fixed a deadlock bug in VolatilityForm.

##v1.0.7
- Resolved the NSE server issue. Now, Option Oracle works seamlessly with NSE.
- Added "Change OI" in the option strike table and option strategy with color code for better visibility and understanding.
- Introduced NSE auto-complete feature in the symbol textbox for a more user-friendly experience.

### Prerequisites

To use Option Oracle, you will need:

- You have a Windows machine.
- You have installed .NET Framework 4.8 or later versions.

### Installation

1. Download the latest release from the [releases](https://github.com/sha37/OptionsOracle/releases/) page.
2. Extract the downloaded file.
3. Download & Copy the all xml files which is under the templates folder from the repository and paste them into `C:\Users\YOUR_USER_NAME\AppData\Roaming\OptionsOracle`.

## Usage

1. Start the application and enter the stock symbol for which you want to analyze options.
2. Choose the desired expiration date, options strategy, and other relevant parameters.
3. Analyze the options chain, Greeks, and potential outcomes based on your chosen strategy.
4. Manage your portfolio and track your trades using the portfolio management features.

## Contributing

We welcome contributions from the community to help improve Option Oracle. If you would like to contribute, please follow these steps:

1. Fork this repository.
2. Create a branch: `git checkout -b <branch_name>`.
3. Make your changes and commit them: `git commit -m '<commit_message>'`
4. Push to the original branch: `git push origin <project_name>/<location>`
5. Create the pull request.

## Acknowledgement

Special thanks to OpenAI's ChatGPT which assisted in resolving some of the issues faced during the development of this project.

## Disclaimer

Please note that I am not the original author of Option Oracle. I downloaded the code from Samoasky's repository and made some modifications to rectify some issues out of my own curiosity. This is not my original code.

This software is provided under the MIT License. Feel free to modify the code, add functionality for other servers, and fix any issues you may encounter.


## License

Option Oracle is released under the [MIT License](LICENSE).
