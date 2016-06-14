(function () {
    "use strict";

    angular.module(APPNAME)
        .controller('userProfileController', userProfileController);

    userProfileController.$inject = ['$scope', '$baseController', "$userProfileService"];

    function userProfileController(
        $scope
        , $baseController
        , $userProfileService
         ) {

        var vm = this;
        vm.headingInfo = "Angular 101";
        vm.item = null;
        vm.showNewEmployeeErrors = false;
        vm.userId = $('#userId').val();
        vm.isFollowing = $('#isFollowing').val();
        vm.isLoggedIn = $('#isLoggedIn').val();


        vm.$userProfileService = $userProfileService;
        vm.$scope = $scope;

        vm.receiveItems = _receiveItems;
        vm.onEmpError = _onEmpError;



        $baseController.merge(vm, $baseController);


        vm.notify = vm.$userProfileService.getNotifier($scope);

        render();

        function render() {
            vm.$userProfileService.ById(vm.userId, vm.receiveItems, vm.onEmpError);
            vm.$systemEventService.broadcast("updateFollowers");
            console.log("whats the deal" + vm.currentUser);

        }
        _init();

        function _init() {
            vm.$systemEventService.listen("testSuccess", render);
        }

        function _receiveItems(data) {

            vm.notify(function () {
                vm.item = data.item;
            });
            console.log(data);

        }

        function _onEmpError(jqXhr, error) {
            console.error(error);
        }
    }
})();