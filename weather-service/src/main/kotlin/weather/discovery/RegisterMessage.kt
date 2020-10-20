package weather.discovery

class RegisterMessage(var handlers: List<String>) {
    var service: String =
            String.format("%s:%s",
                    System.getenv("REGISTER_AS"),
                    System.getenv("PORT")
            )

}