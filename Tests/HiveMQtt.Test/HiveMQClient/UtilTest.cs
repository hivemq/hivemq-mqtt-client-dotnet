namespace HiveMQtt.Test.HiveMQClient;

using Xunit;
using HiveMQtt.Client;

public class UtilTest
{
    [Fact]
    public void SingleLevelWildcardMatch()
    {
        // The plus sign (‘+’ U+002B) is a wildcard character that matches only one topic level.
        //
        // The single-level wildcard can be used at any level in the Topic Filter, including first and last levels. Where it is used, it MUST occupy an entire level of the filter [MQTT-4.7.1-2]. It can be used at more than one level in the Topic Filter and can be used in conjunction with the multi-level wildcard.
        //
        // For example, “sport/tennis/+” matches “sport/tennis/player1” and “sport/tennis/player2”, but not “sport/tennis/player1/ranking”. Also, because the single-level wildcard matches only a single level, “sport/+” does not match “sport” but it does match “sport/”.
        //
        // ·          “+” is valid
        // ·         “+/tennis/#” is valid
        // ·         “sport+” is not valid
        // ·         “sport/+/player1” is valid
        // ·         “/finance” matches “+/+” and “/+”, but not “+”
        bool result;

        // “sport/tennis/+” matches “sport/tennis/player1”
        result = HiveMQClient.MatchTopic("sport/tennis/+", "sport/tennis/player1");
        Assert.True(result);

        // “sport/tennis/+” doesn't match sport/tennis/player1/ranking”
        result = HiveMQClient.MatchTopic("sport/tennis/+", "sport/tennis/player1/ranking");
        Assert.False(result);

        // “sport/+” does not match “sport”
        result = HiveMQClient.MatchTopic("sport/+", "sport");
        Assert.False(result);

        // “sport/+” does match “sport/”
        result = HiveMQClient.MatchTopic("sport/+", "sport/");
        Assert.True(result);

        // "sport/+/player1" matches "sport/tennis/player1"
        result = HiveMQClient.MatchTopic("sport/+/player1", "sport/tennis/player1");
        Assert.True(result);

        // "/finance" matches “/+”
        result = HiveMQClient.MatchTopic("/+", "/finance");
        Assert.True(result);

        // "/finance" doesn't match “+”
        result = HiveMQClient.MatchTopic("+", "/finance");
        Assert.False(result);

        // A subscription to “+/monitor/Clients” will not receive any messages published to “$SYS/monitor/Clients”
        result = HiveMQClient.MatchTopic("+/monitor/Clients", "$SYS/monitor/Clients");
        Assert.False(result);

        // A subscription to “$SYS/monitor/+” will receive messages published to “$SYS/monitor/Clients”
        result = HiveMQClient.MatchTopic("$SYS/monitor/+", "$SYS/monitor/Clients");
        Assert.True(result);
    }

    [Fact]
    public void MultiLevelWildcardMatch()
    {
        // From: https://docs.oasis-open.org/mqtt/mqtt/v5.0/os/mqtt-v5.0-os.html#_Toc3901244
        // # The number sign (‘#’ U+0023) is a wildcard character that matches any number of levels within a topic. The multi-level wildcard represents the parent and any number of child levels. The multi-level wildcard character MUST be specified either on its own or following a topic level separator. In either case it MUST be the last character specified in the Topic Filter [MQTT-4.7.1-1].
        //
        // For example, if a Client subscribes to “sport/tennis/player1/#”, it would receive messages published using these Topic Names:
        // ·         “sport/tennis/player1”
        // ·         “sport/tennis/player1/ranking
        // ·         “sport/tennis/player1/score/wimbledon”
        //
        // ·         “sport/#” also matches the singular “sport”, since # includes the parent level.
        // ·         “#” is valid and will receive every Application Message
        // ·         “sport/tennis/#” is valid
        // ·         “sport/tennis#” is not valid
        // ·         “sport/tennis/#/ranking” is not valid
        bool result;

        // “sport/tennis/#” matches “sport/tennis/player1”
        result = HiveMQClient.MatchTopic("sport/tennis/player1/#", "sport/tennis/player1");
        Assert.True(result);

        // “sport/tennis/#” matches “sport/tennis/player1/ranking”
        result = HiveMQClient.MatchTopic("sport/tennis/player1/#", "sport/tennis/player1/ranking");
        Assert.True(result);

        // “sport/tennis/+” matches “sport/tennis/player1/ranking”
        result = HiveMQClient.MatchTopic("sport/tennis/player1/#", "sport/tennis/player1/score/wimbledon");
        Assert.True(result);

        // “sport/#” also matches the singular “sport”, since # includes the parent level.
        result = HiveMQClient.MatchTopic("sport/#", "sport");
        Assert.True(result);

        // “#” is valid and will receive every Application Message
        result = HiveMQClient.MatchTopic("#", "any/and/all/topics");
        Assert.True(result);

        // Invalid multi-level wildcards
        Assert.Throws<ArgumentException>(() => HiveMQClient.MatchTopic("invalid/mlwc#", "sport/tennis/player1/ranking"));

        // “sport/tennis/#/ranking” is not valid
        Assert.Throws<ArgumentException>(() => HiveMQClient.MatchTopic("sport/tennis/#/ranking", "sport/tennis/player1/ranking"));
        Assert.Throws<ArgumentException>(() => HiveMQClient.MatchTopic("/#/", "sport/tennis/player1/ranking"));

        // “sport/tennis#” is not valid
        Assert.Throws<ArgumentException>(() => HiveMQClient.MatchTopic("sports/tennis#", "sport/tennis/player1/ranking"));

        // A subscription to “#” will not receive any messages published to a topic beginning with a $
        result = HiveMQClient.MatchTopic("#", "$SYS/broker/clients/total");
        Assert.False(result);

        // A subscription to “$SYS/#” will receive messages published to topics beginning with “$SYS/”
        result = HiveMQClient.MatchTopic("$SYS/#", "$SYS/broker/clients/total");
        Assert.True(result);

    }
}
