module.exports = {
    platform: "azure",
    endpoint: 'https://dev.azure.com/ullrichsoftware/',
    repositories: ["Tools/FeedToMastodon"],
    hostRules: [
        {
            "azureAutoApprove": true,
            "automerge": true
        }
    ]
}