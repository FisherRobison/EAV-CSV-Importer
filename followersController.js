
(function () {
    "use strict";

    angular.module(APPNAME)
        .controller('followerController', followerController);

    followerController.$inject = ['$scope', '$baseController', "$followerService", "$systemEventService", "$alertService"];

    function followerController(
        $scope
        , $baseController
        , $followerService
        , $systemEventService
        , $alertService
         ) {


        var vm = this;
        vm.headingInfo = "Angular 101";
        vm.item = null;
        vm.showNewEmployeeErrors = false;
        vm.userId = $('#userId').val();
        vm.isFollowing = $('#isFollowing').val();
        vm.isLoggedIn = $('#isLoggedIn').val();
        vm.currentUser = $('#currentId').val();



        vm.$followerService = $followerService;
        vm.$scope = $scope;

        vm.receiveItems = _receiveItems;
        vm.onEmpError = _onEmpError;
        vm.onFollow = _onfollow;
        vm.onUnfollow = _onUnfollow;
        vm.unFollowSuccess = _unFollowSuccess;
        vm.FollowSuccess = _FollowSuccess;
        vm.myFollowers = _myFollowers;
        vm.followersTabPressed = _followerTabPressed;
        vm.followingTabPressed = _followingTabPressed;
        vm.goToUser = _goToUser;


        $baseController.merge(vm, $baseController);

        vm.notify = vm.$followerService.getNotifier($scope);

        _myFollowers();

        function _myFollowers() {

            vm.$followerService.myFollower(vm.userId, vm.receiveItems, vm.onEmpError);
        }
        _init();

        function _init() {

            vm.$systemEventService.listen("updateFollowers", _myFollowers);
        }


        function _receiveItems(data) {
            console.log(data);

            vm.notify(function () {
                vm.items = data.items;
            });

        }

        function _onEmpError(jqXhr, error) {
            console.error(error);
        }
        function _onfollow() {
            var payload = {
                FollowedId: vm.userId
            }
            vm.$followerService.followUser(payload, vm.FollowSuccess, vm.onEmpError);
        }
        function _FollowSuccess() {
            vm.isFollowing = true;

            vm.$alertService.success("Following!!");
            vm.$systemEventService.broadcast("testSuccess");


        }

        function _onUnfollow() {
            vm.$followerService.unfollowUser(vm.userId, vm.unFollowSuccess);

        }

        function _unFollowSuccess() {
            vm.isFollowing = false;

            vm.$alertService.success("Unfollowed!");
            vm.$systemEventService.broadcast("testSuccess");

        }

        function _followerTabPressed() {

            vm.$followerService.myFollower(vm.userId, vm.receiveItems, vm.onEmpError);
        }

        function _followingTabPressed() {
            vm.$followerService.myFollowering(vm.userId, vm.receiveItems, vm.onEmpError);

        }

        function _goToUser() {
        }

    }
})();