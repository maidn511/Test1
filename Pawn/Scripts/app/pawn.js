var app = angular.module('Pawn', ['ngTouch', 'moment-picker', 'ngSanitize', 'localytics.directives']);
app.value('signalRServer', 'http://pawn.com/');

app.run(function ($rootScope, $log) {
    var lastAccessApp = new Date();

    $rootScope.$watch(function watchIdleInterval() {
        var date = new Date();
        var nowDate = date - lastAccessApp;
        //set 20 phút
        if (20 * 60 * 1e3 < nowDate) {
            var scope = angular.element($("#pawnApp")).scope();
            window.location.href = location.pathname;
        }
        lastAccessApp = date;
    });
});

app.directive('formatCurrencyInput',
    function ($filter) {
        'use strict';
        return {
            require: '?ngModel',
            link: function (scope, elem, attrs, ctrl) {
                if (!ctrl) {
                    return;
                }

                ctrl.$formatters.unshift(function () {
                    return $filter('number')(ctrl.$modelValue);
                });

                ctrl.$parsers.unshift(function (viewValue) {
                    var plainNumber = viewValue.replace(/[\,\.]/g, ''),
                        b = $filter('number')(plainNumber);

                    elem.val(b);

                    return plainNumber;
                });
            }
        }
    });

app.config(['momentPickerProvider', function (momentPickerProvider) {
    momentPickerProvider.options({
        /* Picker properties */
        locale: 'en',
        format: 'L LTS',
        minView: 'decade',
        maxView: 'minute',
        startView: 'day',
        autoclose: true,
        today: true,
        keyboard: false,

        /* Extra: Views properties */
        leftArrow: '&larr;',
        rightArrow: '&rarr;',
        yearsFormat: 'YYYY',
        monthsFormat: 'MMM',
        daysFormat: 'D',
        hoursFormat: 'HH:[00]',
        minutesFormat: moment.localeData().longDateFormat('LT').replace(/[aA]/, ''),
        secondsFormat: 'ss',
        minutesStep: 5,
        secondsStep: 1
    });
}]);

//app.directive('selectpicker', function ($timeout) {
//    return {
//        restrict: 'C',
//        require: 'ngModel',
//        link: function (scope, element, attrs, ngModel) {
//            $timeout(function () {
//                element.select2();
//            });
//            ngModel.$render = function () {
//                element.select2("val", ngModel.$viewValue);
//            }
//            element.on('change', function () {
//                scope.$apply(function () {
//                    ngModel.$setViewValue(element.select2("val"));
//                });
//            })
//        }
//    };
//});

app.directive('iCheck', function ($timeout, $parse) {
    return {
        require: 'ngModel',
        link: function ($scope, element, $attrs, ngModel) {
            return $timeout(function () {
                var value;
                value = $attrs['value'];

                $scope.$watch($attrs['ngModel'], function (newValue) {
                    $(element).iCheck('update');
                });

                $scope.$watch($attrs['ngDisabled'], function (newValue) {
                    $(element).iCheck(newValue ? 'disable' : 'enable');
                    $(element).iCheck('update');
                })

                return $(element).iCheck({
                    checkboxClass: "icheckbox_flat-green",
                    radioClass: "iradio_flat-green"
                }).on('ifChanged', function (event) {
                    if ($(element).attr('type') === 'checkbox' && $attrs['ngModel']) {
                        $scope.$apply(function () {
                            return ngModel.$setViewValue(event.target.checked);
                        })
                    }
                }).on('ifClicked', function (event) {
                    if ($(element).attr('type') === 'radio' && $attrs['ngModel']) {
                        return $scope.$apply(function () {
                            if (ngModel.$viewValue != value)
                                return ngModel.$setViewValue(value);
                            else
                                ngModel.$setViewValue(null);
                            ngModel.$render();
                            return
                        });
                    }
                });
            });
        }
    };
});

function isEmpty(value) {
    return angular.isUndefined(value) || value === '' || value === null || value !== value;
}

app.directive('ngMin', function () {
    return {
        restrict: 'A',
        require: 'ngModel',
        link: function (scope, elem, attr, ctrl) {
            scope.$watch(attr.ngMin, function () {
                ctrl.$setViewValue(ctrl.$viewValue);
            });
            var minValidator = function (value) {
                var min = +attr.ngMin || 0;
                if (!isEmpty(value) && value < min) {
                    ctrl.$setValidity('ngMin', false);
                    return undefined;
                } else {
                    ctrl.$setValidity('ngMin', true);
                    return value;
                }
            };

            ctrl.$parsers.push(minValidator);
            ctrl.$formatters.push(minValidator);
        }
    };
});

app.directive('ngMax', function () {
    return {
        restrict: 'A',
        require: 'ngModel',
        link: function (scope, elem, attr, ctrl) {
            scope.$watch(attr.ngMax, function () {
                ctrl.$setViewValue(ctrl.$viewValue);
            });
            var maxValidator = function (value) {
                var max = scope.$eval(attr.ngMax) || Infinity;
                if (!isEmpty(value) && value > max) {
                    ctrl.$setValidity('ngMax', false);
                    return undefined;
                } else {
                    ctrl.$setValidity('ngMax', true);
                    return value;
                }
            };

            ctrl.$parsers.push(maxValidator);
            ctrl.$formatters.push(maxValidator);
        }
    };
});


app.directive('popover', function($compile){
    return {
        restrict : 'A',
        link : function(scope, elem){
            var content = $(elem).find("#popover-content").html();
            var compileContent = $compile(content)(scope);
            var title = $(elem).find("#popover-head").html();
            var options = {
                content: compileContent,
                html: true,
                title: title
            };
            
            $(elem).popover(options);
        }
    }
});

app.filter('capitalize', function () {
    return function (input) {
        return (!!input) ? input.charAt(0).toUpperCase() + input.substr(1).toLowerCase() : '';
    }
});
app.filter('moneyCharacter', function () {
    return function (input) {
        var res = (!!input) ? DocTienBangChu(input) : '';
        return res;
    }
});
app.filter('dateCharacter', function () {
    return function (input, format) {
        var date = new Date();
        if (format == 'jsonDate') date = parseInt(input.substr(6)); //jsondate
        return 'Ngày ' + PadLeftRight(date.getDate(), '00', 'L') + ' Tháng ' + PadLeftRight(date.getMonth(), '00', 'L') + ' Năm ' + date.getFullYear();
    }
});
app.filter('dateNow', function () {
    return function (input, format) {
        return moment().format(format);
    }
});
app.filter('total', function () {
    return function (input, property) {
        var i = input.length;
        var total = 0;
        while (i--)
            total += input[i][property];
        return total;
    }
});

app.filter('jsonDate', ['$filter', function ($filter) {
    return function (input, format) {
        return (input)
            ? $filter('date')(parseInt(input.substr(6)), format)
            : '';
    };
}]);

app.controller("pawnController", function ($scope,$filter) {
    //$scope.myInit = function (name, address, id) {
    //    $scope.Store = {
    //        Name: name,
    //        Address: address,
    //        Id:id
    //    }
    //}
    //LoadData
    // Get List Stores and Store
    $scope.LoadData = function () {
        var url = $('#pawnApp').attr('data-load-stores-url'); if (url == undefined) return;
        window.Post_AjaxCaller(url, {}, function (respone) {
            $scope.Store = respone.store;
            $scope.StoreList = respone.storeList;
        }, function () { });
    }
    
  
    // Assign Store 


    //Choose Store

});


app.directive('ngMin9Digit', function () {
    return {
        restrict: 'A',
        require: 'ngModel',
        link: function (scope, elem, attr, ctrl) {
            scope.$watch(attr.ngMin9Digit, function () {
                ctrl.$setViewValue(ctrl.$viewValue);
            });
            var min9DigitValidate = function (value) {
                if (!isEmpty(value) && value.length < 9) {
                    ctrl.$setValidity('ngMin9Digit', false);
                    return undefined;
                } else {
                    ctrl.$setValidity('ngMin9Digit', true);
                    return value;
                }
            };

            ctrl.$parsers.push(min9DigitValidate);
            ctrl.$formatters.push(min9DigitValidate);
        }
    };
});


app.factory('signalRHubProxy', ['$rootScope', 'signalRServer',
    function ($rootScope, signalRServer) {
        function signalRHubProxyFactory(serverUrl, hubName, startOptions) {
            var connection = $.hubConnection(signalRServer);
            var proxy = connection.createHubProxy(hubName);
            connection.start(startOptions).done(function () { });

            return {
                on: function (eventName, callback) {
                    proxy.on(eventName, function (result) {
                        $rootScope.$apply(function () {
                            if (callback) {
                                callback(result);
                            }
                        });
                    });
                },
                off: function (eventName, callback) {
                    proxy.off(eventName, function (result) {
                        $rootScope.$apply(function () {
                            if (callback) {
                                callback(result);
                            }
                        });
                    });
                },
                invoke: function (methodName, callback) {
                    proxy.invoke(methodName)
                        .done(function (result) {
                            $rootScope.$apply(function () {
                                if (callback) {
                                    callback(result);
                                }
                            });
                        });
                },
                connection: connection
            };
        };

        return signalRHubProxyFactory;
    }]);