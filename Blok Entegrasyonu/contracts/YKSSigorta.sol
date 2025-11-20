// SPDX-License-Identifier: MIT
pragma solidity ^0.8.0;

contract YKSSigorta {
    uint256 public examDate; 
    mapping(address => bool) public policies;
    
    constructor(uint256 _examDate) {
        examDate = _examDate;
    }

    function mintPolicy(address student) public {
        require(block.timestamp < examDate, "Sinav zamani gecti, police kesilemez.");
        policies[student] = true;
    }
    
    function checkInsurance(address student) public view returns (bool) {
        return policies[student];
    }
}